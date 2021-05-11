using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeleSharp.TL;
using TeleSharp.TL.Messages;
using TLSharp.Core;

namespace TelegramSessionKeeper
{
    class Program
    {
        private const int OFFICIAL_ACCOUNT_ID = 777000;
        private const string DEFAULT_SESSION_FILE = "./session.dat";

        static int Main(string[] args)
        {
            var apiIdOption = new Option<int>(
                alias: "--api-id",
                description: "Telegram application API ID"
            );
            var apiHashOption = new Option<string>(
                alias: "--api-hash",
                description: "Telegram application API hash"
            );
            var phoneOption = new Option<string>(
                alias: "--phone",
                description: "Phone number in international format"
            );
            var sessionOption = new Option<FileInfo>(
                alias: "--session",
                description: "Path to session file"
            );
            var authHashOption = new Option<string>(
                alias: "--auth-hash",
                description: "Authentication code hash"
            );
            var authCodeOption = new Option<string>(
                alias: "--auth-code",
                description: "Authentication code"
            );

            var rootCommand = new RootCommand(
                description: "A tool for creating and using Telegram backup sessions"
            );

            var createCommand = new Command(
                name: "create",
                description: "Create a new Telegram session and request for authentication code"
            );
            createCommand.AddOption(apiIdOption);
            createCommand.AddOption(apiHashOption);
            createCommand.AddOption(phoneOption);
            createCommand.AddOption(sessionOption);
            createCommand.Handler = CommandHandler.Create<int, string, string, FileInfo>(HandleCreateAsync);

            var authCommand = new Command(
                name: "auth",
                description: "Authenticate a newly created session with authentication code"
            );
            authCommand.AddOption(apiIdOption);
            authCommand.AddOption(apiHashOption);
            authCommand.AddOption(sessionOption);
            authCommand.AddOption(phoneOption);
            authCommand.AddOption(authHashOption);
            authCommand.AddOption(authCodeOption);
            authCommand.Handler = CommandHandler.Create<int, string, FileInfo, string, string, string>(HandleAuthAsync);

            var readCommand = new Command(
                name: "read",
                description: "Read messages from an authenticated Telegram session"
            );
            readCommand.AddOption(apiIdOption);
            readCommand.AddOption(apiHashOption);
            readCommand.AddOption(sessionOption);
            readCommand.Handler = CommandHandler.Create<int, string, FileInfo>(HandleReadAsync);

            rootCommand.AddCommand(createCommand);
            rootCommand.AddCommand(authCommand);
            rootCommand.AddCommand(readCommand);

            return rootCommand.InvokeAsync(args).Result;
        }

        private static async Task HandleCreateAsync(int apiId, string apiHash, string phone, FileInfo session)
        {
            EnsureDefaultSessionDoesntExist();
            if (session.Exists)
                throw new Exception("Session file already exists");

            var client = await CreateClientAsync(apiId, apiHash);

            string codeRequestHash = await client.SendCodeRequestAsync(phone);
            Console.WriteLine($"Authentication code requested. Auth hash (required for auth): {codeRequestHash}");

            // Hacky way to avoid implementing custom store
            File.Move(DEFAULT_SESSION_FILE, session.FullName);
        }

        private static async Task HandleAuthAsync(int apiId, string apiHash, FileInfo session, string phone, string authHash, string authCode)
        {
            EnsureDefaultSessionDoesntExist();
            if (!session.Exists)
                throw new Exception("Session file not found");

            File.Move(session.FullName, DEFAULT_SESSION_FILE);

            try
            {
                var client = await CreateClientAsync(apiId, apiHash);

                await client.MakeAuthAsync(phone, authHash, authCode);
            }
            finally
            {
                File.Move(DEFAULT_SESSION_FILE, session.FullName);
            }
        }

        private static async Task HandleReadAsync(int apiId, string apiHash, FileInfo session)
        {
            EnsureDefaultSessionDoesntExist();
            if (!session.Exists)
                throw new Exception("Session file not found");

            File.Move(session.FullName, DEFAULT_SESSION_FILE);

            try
            {
                var client = await CreateClientAsync(apiId, apiHash);
                if (!client.IsUserAuthorized())
                    throw new Exception("Session not authenticated");

                var dialogs = (TLDialogsSlice)await client.GetUserDialogsAsync();
                var officialDialog = dialogs.Dialogs
                    .FirstOrDefault(item => item.Peer is TLPeerUser peerUser && peerUser.UserId == OFFICIAL_ACCOUNT_ID);

                if (officialDialog is null)
                {
                    Console.WriteLine("Official account dialog not found");
                }
                else
                {
                    var dialogHistory = (TLMessagesSlice)await client.GetHistoryAsync(
                        peer: new TLInputPeerUser
                        {
                            UserId = OFFICIAL_ACCOUNT_ID,
                        },
                        limit: 5
                    );
                    var recentMessages = dialogHistory.Messages
                        .Reverse()
                        .Cast<TLMessage>()
                        .ToArray();

                    Console.WriteLine("Recent messages:");
                    for (int ind = 0; ind < recentMessages.Length; ind++)
                    {
                        Console.WriteLine($"[{ind + 1} / {recentMessages.Length}]: {recentMessages[ind].Message}");

                        if (ind != recentMessages.Length - 1)
                            Console.WriteLine();
                    }
                }
            }
            finally
            {
                File.Move(DEFAULT_SESSION_FILE, session.FullName);
            }
        }

        private static async Task<TelegramClient> CreateClientAsync(int apiId, string apiHash)
        {
            var client = new TelegramClient(apiId: apiId, apiHash: apiHash, dcIpVersion: DataCenterIPVersion.OnlyIPv4);
            await client.ConnectAsync();

            return client;
        }

        private static void EnsureDefaultSessionDoesntExist()
        {
            if (File.Exists(DEFAULT_SESSION_FILE))
                throw new Exception("Default session file already exists");
        }
    }
}
