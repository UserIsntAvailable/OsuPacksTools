# OsuPacksTools
Some tools to facilitate the downloading and unpacking of .osz packs of osu!

## Current state of the app
It turns out that google drive doen't allow you to download a massive amount of files ( I expected tbh ) so I will be doing a torrent downloader instead.
You can use GoogleDriveDownloader but you can download at much like 4-5 months packs each hour so not very efficient.

## TodoList
- TorrentDownloader<br />
- Auto fix the problem of maps with no-ASCII characters<br />
- Print the "state" of the program ( for now the CLI is only blank until the program ends, unless you compile it in debug mode )<br />
- XMLWriter: Write downloaded packs, Write osu folder<br />
- IProgress for GDDownloader.GetFileAsStream<br />
- CMD Option to Write apiKey on Environment Variables<br />
- Unit Testing<br />
- README.md update: Explain app usage<br />
- Create Service: Download pack everytime a new one is added to the google drive folder<br />
- Maybe a osu!.db reader & writer for import beatmaps without the need to open osu!?<br />