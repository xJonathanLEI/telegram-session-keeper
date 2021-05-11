# Telegram Session Keeper

This repository contains a cross-platform command line tool written in C# for creating and using backup Telegram session files.

With a backup session file, one can retain access to the corresponding account, without having access to the actual phone number, by fetching authentication code using the said file.

> **NOTE**
>
> Telegram session messages are strictly sequential. You **cannot** use an old session file twice. Copying before using a session file is thus pointless.

## Installation

Install the [NuGet package](https://www.nuget.org/packages/TelegramSessionKeeper/) as a global .NET Core tool:

```sh
$ dotnet tool install --global TelegramSessionKeeper
```

## License

[MIT](./LICENSE)
