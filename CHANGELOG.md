# Changelog

## [2.0.11](https://github.com/Eternal-ll/Ethereal-FAF-Client/releases/tag/2.0.11) (2022-10-23)

**Changelog**

- Added safe handler in patcher for busy files
- Added game logs on 'C:\ProgramData\FAForever\logs\game_{uid}.log'
- Fixed background when hosting

## [2.0.10](https://github.com/Eternal-ll/Ethereal-FAF-Client/releases/tag/2.0.10) (2022-10-23)

**Changelog**

- Fix game patcher
- Fix custom map downloading on game launch
- Fix idle games list not updating on game launch
- Fix oauth logo
- Host page background now transparent
- Added clean patch initialization (no more dependant on exist patch folder, but still using auto-game detect method)
- Added game filters: OnlyGeneratedMaps / HidePrivateLobbies / EnableMapsBlacklist
- Added players ratinsg to idle/live custom/coop games
- Carefully closing ice adapter
- Host page moved into navigation frame
- Code cleanup


**FAQ**

How to hide specific map?

- Right click on game card
- Click on "Ban map"

![](https://cdn.discordapp.com/attachments/378156367933997056/1033702143745605632/unknown.png)

![](https://cdn.discordapp.com/attachments/926400628803113020/1033703179361198221/unknown.png)

## [2.0.9](https://github.com/Eternal-ll/Ethereal-FAF-Client/releases/tag/2.0.9) (2022-10-22)

#### Changelog

- Full support of IRC chat
- Open private DM with player
- Filter channel users
- Support emoji syntax `:emoji:` on chat input
- Remember last user channels for next IRC session
- Parallel initilization of patch watcher (faster launch)
- Updated ICE to 3.2.2


#### FAQ

**How to join channel/s?**

- Use chat input **/join #channel1, #channel2, ..., <#channel>**
- Use channel/user input <#channel> and press Enter

**How to leave channel/s?**

- Use chat input **/part #channel1, #channel2, ..., <#channel>**
- Use leave button, hover on required channel in the sidebar list and press "Leave" button

**RAW IRC commands**

- Use slash '/'

Example:
- UI: /join #channel
- IRC: JOIN #channel

**Emoji Sample:**
1. Write `:tada:`
2. Converted to 🎉
![](https://cdn.discordapp.com/attachments/926400628803113020/1033048668187676672/Animation.gif)

**How to copy message?**

1. Right click on message
![def8bd11-5516-463d-9638-5f05e3af1e23-image.png](https://forum.faforever.com/assets/uploads/files/1666426882009-def8bd11-5516-463d-9638-5f05e3af1e23-image.png) 
2. Click on "Copy text"
3. Done!
![12357639-7b30-4ab5-b7a7-cac19f29cb10-image.png](https://forum.faforever.com/assets/uploads/files/1666426892771-12357639-7b30-4ab5-b7a7-cac19f29cb10-image.png)