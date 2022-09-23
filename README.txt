This is a portfolio repository containing the C# scripts written for my game, Sons of Ra, organized as they are within the project source.

Sons of Ra is a game developed and published by myself and classmates from college. Originally a class project, after a strongly positive reception we decided to continue development with intent to publish. The game appeared at conferences including PAX West, Dreamhack, and E3, and has won multiple awards. The game released after 3 years of development in April 2021. Sons of Ra was developed in Unity.



The features contained within that were the results of my most significant efforts include:
	- The main Player script, which contains the driver for the radial UI that is used for all
	    in-game actions (SonsOfRaScripts/Player/HumanPlayerController.cs, PlayerController.cs)
	- Controllers for our rudimentary AI opponent (scripts within SonsOfRaScripts/Player/AI/)
	- Networked Multiplayer, allowing two players to connect via Steam's API to play together 
	    remotely via peer-to-peer networking (distributed throughout the source code and in multiple scripts)
	- Controller code for the vast majority of non-gameplay UI systems
	- Unit AI controllers and behaviors (SonsOfRaScripts/UnitBehaviors/)
