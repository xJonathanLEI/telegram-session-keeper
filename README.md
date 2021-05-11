# Telegram Session Keeper

[![NuGet](https://img.shields.io/nuget/v/TelegramSessionKeeper.svg?style=flat&color=green)](https://www.nuget.org/packages/TelegramSessionKeeper/)

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

## Prerequisite

To use the tool you must first create a [Telegram Application](https://core.telegram.org/api/obtaining_api_id) and obtain a pair of _API ID_ and _API Hash_. You **MUST** use the same application for creating and later using the session. It is therefore recommended that you backup both the session file and the key pair.

## License

[MIT](./LICENSE)
