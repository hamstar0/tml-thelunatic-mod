namespace TheLunatic.NetProtocol {
	enum NetProtocolTypes : byte {
		RequestModSettings,
		RequestModData,
		ModSettings,
		ModData,
		EndSign,
		GiveMaskToServer,
		GiveMaskToClient
	}
}
