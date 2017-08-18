namespace TheLunatic.NetProtocol {
	public enum NetProtocolTypes : byte {
		RequestModSettings,
		RequestModData,
		ModSettings,
		ModData,
		EndSign,
		GiveMaskToServer,
		GiveMaskToClient
	}
}
