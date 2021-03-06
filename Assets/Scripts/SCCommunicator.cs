﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SCCommunicator : MonoBehaviour {

	//Global user varaibles
	public static string gameName = "";
	public static string userName = "";
	public static string password = "";
	public static int numberOfPlayers = 0;
	public static bool hasServer = false;
	public static bool automaticallyReconnect = false;

	// Commands
	public static List<SCCommandBehaviour> commands = new List<SCCommandBehaviour>();

	public static void addCommand(string command, Action info, int id = -1){
		addCommand(new SCCommandBehaviour(command, info, id));
	}

	public static void addCommand(string command, Action<SCMessageInfo> info, int id = -1){
		addCommand(new SCCommandBehaviour(command, info, id));
	}

	public static void addCommand(SCCommandBehaviour commandBehaviour){
		commands.Add(commandBehaviour);
	}

	public static void removeCommands(int id){
		commands.RemoveAll(x => x.id == id);
	}

	public static void fireCommand(string message){
		string command = SCNetworkUtil.getCommand(message);
		SCMessageInfo info = SCNetworkUtil.decodeMessage(message);
		for(int i = 0; i < commands.Count; ++i){
			if(commands[i].command == command){
				commands[i].executeCallback(info);
			}
		}
	}
}
