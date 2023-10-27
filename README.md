# ACNH-Pattern-Research
Research on ACNH pattern data for fixing some save editor bugs.  
This is in reference to fork: https://github.com/lottehime/NHSE


### What is it?
I noticed when using NHSE and ACNHDesignPatternEditor that under certain conditions, importing pattern files (`*.nhd` or `*.acnh`) did not seem to set the patterns as owned by the player and thus they were not editable in-game, much like if they were QR codes or from the pattern exchange.

I also noticed that for some pattern slots, the previous pattern name would remain in place of the imported one.

It bugged me.

So, I hacked together a fix and added it to a fork of NHSE which you can find here: https://github.com/lottehime/NHSE (source only)
I also added a build to this repo while waiting for the pull request to resolve, so that you (and I) can use it without waiting.


## Research
Pattern data is found in `main.dat` within the save, as are flags for if the player has edited the pattern slot.  
An ID for the player and the town/island reside in `personal.dat` within the save.

The pattern 'IsEdited' flags begin at offset `0x8BE260` starting with design 1 of 100. Each flag is a single byte and each byte begins as `0xFF` and is changed to `0x00` when the player edits that pattern slot.  
Add `0x64` to that offset to get to the design PRO patterns at offset `0x8BE2C4` starting with design 1 of 100. The bytes follow the same pattern as above.  
This fixes the issue of imported patterns retaining the name of the pattern originally in that slot.

The issue of pattern ownership and editability is caused by the pattern data not being written to `main.dat` with the players IDs correctly.  
The players ID information can be found within `personal.dat` starting at offset `0xB0B8`. It can also be found in other locations, such as `main.dat` before the pattern offsets.  
It is made up of two primary parts: the `PlayerID` and `TownID`, which are each made up of two parts; the `ID` and the `Name`.

The TownID comes first, with the ID starting at offset `0xB0B8` for 4 bytes, followed by the name starting at `0xB0BC` for 20 bytes.  
The 20 bytes represent the name as a 10 character long name with `0x00` between each character.

The PlayerID comes next, with the ID starting at offset `0xB0D4` for 4 bytes, followed by the name starting at `0xB0D8` for 20 bytes.  
The 20 bytes represent the name as a 10 character long name with `0x00` between each character.

Storing these as a two byte arrays of `0x18` or 24 bytes length starting at each of their respective offsets allows us to then write them into the data for the patterns as we insert them into `main.dat`.

Patterns in `main.dat` start at offset `0x1E3968` and flow into one another.  
Complete untrimmed pattern data is 680 bytes long, starting with a 16 byte hash and ending with 3 trailing `0x00` bytes after the image data.  
This format matches what you will find in `*.acnh` files from https://acpatterns.com/, files from other editors may be trimmed.  
We can use a `*.acnh` file to isolate the data we are interested in.

The structure of the pattern file is roughly as follows:  
`0x000 -> 0x00F`:   pattern hash - (16 bytes long)  
`0x010 -> 0x037`:   pattern name - (20 character name with separating `0x00`, 40 bytes long)  
`0x038 -> 0x03B`:   town ID - (4 bytes long)  
`0x03C -> 0x04F`:   town name - (10 character name with separating `0x00`, 20 bytes long)  
`0x050 -> 0x053`:   padding? - (4 bytes long)  
`0x054 -> 0x057`:   player ID - (4 bytes long)  
`0x058 -> 0x06B`:   player name - (10 character name with separating `0x00`, 20 bytes long)  
`0x06C -> 0x06F`:   padding? - (4 bytes long)  
`0x070 -> 0x077`:   unknown flag? - (8 bytes long)  
`0x078 -> 0x2A4`:  palette and pixel data - (not delving into specifics, 557 bytes long)  
`0x2A5 -> 0x2A7`: trailing padding - (3 bytes long)  

If we take the `PlayerID` and `TownID` data extracted from `personal.dat` and inject it at offsets `0x54` and `0x38` respectively, then write these back to their correct location in `main.dat` (main pattern offset + index) we end up with a pattern written with the image we wanted, and the correct player and town IDs. This allows the user to own/edit them in-game.

When you mix the pattern being updated with the players IDs correctly, and the IsEdited flags flipped to edited you get a correctly named and editable pattern imported into your save. Yay!

PRO patterns follows a similar methodology, but with differing offsets. The above concept applied to them also works.  
You can refer to NHSE offsets in the source for more info.

This was fun to find and fix and I hope it is an educational reference in the future.

<!-- BUY ME A COFFEE -->
## Help Support More Like This

<a href="https://www.buymeacoffee.com/lottehime" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>
