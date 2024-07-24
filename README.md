# DSDeaths2
Based on Quidrex's DSDeaths project
https://github.com/Quidrex/DSDeaths



App Icon (https://icons8.com/icons/set/skull--static) from [icons8.com](https://icons8.com/)



1. Start a Souls game
   1. WARNING - This may not work and you may risk bans if you use this with Elden Ring while Easy Anti-Cheat (EAC) is running
   2. WARNING - You may also need to start Elden Ring without EAC running
2. Load a character
3. Start this app
4. It should find the game process and start listening for deaths.
5. Deaths will write to a game-specific TXT file (not character specific) that will be in the same location as the main EXE 
   1. **deaths_{gameName}.txt**
   2. i.e. deaths_DarkSoulsRemastered.txt
6. You may want to take your own manual backups of the files if you want to track deaths per character. The file will be overwritten with deaths from whatever character you are currently playing.

