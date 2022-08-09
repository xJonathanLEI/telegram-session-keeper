# Telegram Session Keeper

This repository contains a cross-platform command line tool written in Rust for creating and using backup Telegram session files.

With a backup session file, one can retain access to the corresponding account, without having access to the actual phone number, by fetching authentication code using the said file.

## Installation

With Rust toolchain installed:

```sh
$ cargo install --locked --git https://github.com/xJonathanLEI/telegram-session-keeper
```

## Prerequisite

To use the tool you must first create a [Telegram Application](https://core.telegram.org/api/obtaining_api_id) and obtain a pair of _API ID_ and _API Hash_. You **MUST** use the same application for creating and later using the session. It is therefore recommended that you backup both the session file and the API ID & hash.

## License

Licensed under either of

- Apache License, Version 2.0 ([LICENSE-APACHE](./LICENSE-APACHE) or <http://www.apache.org/licenses/LICENSE-2.0>)
- MIT license ([LICENSE-MIT](./LICENSE-MIT) or <http://opensource.org/licenses/MIT>)

at your option.
