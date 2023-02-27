package utils

func VerifyApiKey(ApiKey string) bool {
	cfg := LoadConfig(ADDRESS)
	return cfg.ApiKey == ApiKey
}
