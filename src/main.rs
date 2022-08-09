use std::{
    io::{BufRead, Write},
    path::PathBuf,
    process,
};

use anyhow::Result;
use colored::Colorize;
use grammers_client::{Client, Config, SignInError};
use grammers_session::Session;

use clap::Parser;

const OFFICIAL_ACCOUNT_ID: i64 = 777000;
const MAX_MESSAGE_COUNT: usize = 10;

#[derive(Debug, Parser)]
#[clap(author, version, about)]
struct Cli {
    #[clap(long, help = "API id")]
    api_id: i32,
    #[clap(long, help = "API hash")]
    api_hash: String,
    #[clap(long, help = "Path to session file")]
    session: PathBuf,
}

#[tokio::main]
async fn main() {
    if let Err(err) = Cli::parse().run().await {
        eprintln!("Error: {}", err);
        process::exit(1);
    }
}

impl Cli {
    pub async fn run(self) -> Result<()> {
        let mut client = Client::connect(Config {
            session: Session::load_file_or_create(&self.session)?,
            api_id: self.api_id,
            api_hash: self.api_hash.clone(),
            params: Default::default(),
        })
        .await?;

        println!("Connected");

        if !client.is_authorized().await? {
            println!("Not authorized");
            let phone = prompt("Enter your phone number (international format): ")?;
            let token = client
                .request_login_code(&phone, self.api_id, &self.api_hash)
                .await?;
            let code = prompt("Enter the code you received: ")?;
            let signed_in = client.sign_in(&token, &code).await;
            match signed_in {
                Err(SignInError::PasswordRequired(password_token)) => {
                    let hint = password_token
                        .hint()
                        .map(|v| v.to_owned())
                        .unwrap_or_else(String::new);
                    let password = prompt(&format!("Enter the password (hint {}): ", &hint))?;

                    client
                        .check_password(password_token, password.trim())
                        .await?;
                }
                Ok(_) => (),
                Err(e) => panic!("{}", e),
            };

            println!("Signed in!");
            client.session().save_to_file(&self.session)?;
        } else {
            println!("Already signed in");
        }

        println!("Looking for chat with official Telegram account");
        let mut dialogs = client.iter_dialogs();
        while let Some(dialog) = dialogs.next().await? {
            let chat = dialog.chat();
            if chat.id() == OFFICIAL_ACCOUNT_ID {
                println!("Official chat found");

                let mut messages = vec![];

                let mut msgs = client.iter_messages(chat);
                while let Some(msg) = msgs.next().await? {
                    messages.push(msg);
                    if messages.len() == MAX_MESSAGE_COUNT {
                        break;
                    }
                }

                messages
                    .into_iter()
                    .enumerate()
                    .rev()
                    .for_each(|(ind, msg)| {
                        let text = msg.text();
                        println!("{}", format!(">> [{}] {}", ind, msg.date()).bright_yellow());
                        println!("{}", text);
                    });
            }
        }

        Ok(())
    }
}

fn prompt(message: &str) -> Result<String> {
    let stdout = std::io::stdout();
    let mut stdout = stdout.lock();
    stdout.write_all(message.as_bytes())?;
    stdout.flush()?;

    let stdin = std::io::stdin();
    let mut stdin = stdin.lock();

    let mut line = String::new();
    stdin.read_line(&mut line)?;
    Ok(line)
}
