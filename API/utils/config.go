package utils

import (
	"github.com/magiconair/properties"
)

type Config struct {
	ApiKey   string
	ShopCode string
	Username string
	Password string
	Hostname string
	Dbname   string
}

func LoadConfig(path string) Config {
	cfg := properties.MustLoadFile(path, properties.UTF8)

	APIKEY := cfg.GetString("API.Key", "")
	SHOPCODE := cfg.GetString("SHOP_CODE", "")
	USERNAME := cfg.GetString("USERNAME", "")
	PASSWORD := cfg.GetString("PASSWORD", "")
	HOSTNAME := cfg.GetString("HOSTNAME", "")
	DBNAME := cfg.GetString("DBNAME", "")

	data := Config{
		ApiKey:   APIKEY,
		ShopCode: SHOPCODE,
		Username: USERNAME,
		Password: PASSWORD,
		Hostname: HOSTNAME,
		Dbname:   DBNAME,
	}

	return data
}

func LoadDatabase() (string, string, string, string) {
	cfg := LoadConfig(ADDRESS)
	Username := cfg.Username
	Password := cfg.Password
	Hostname := cfg.Hostname
	Dbname := cfg.Dbname
	return Username, Password, Hostname, Dbname
}

func LoadDatabase_SmartSelf() (string, string, string, string) {
	cfg := properties.MustLoadFile(ADDRESS, properties.UTF8)

	USERNAME_SMARTSELF := cfg.GetString("USERNAME_SMARTSELF", "")
	PASSWORD_SMARTSELF := cfg.GetString("PASSWORD_SMARTSELF", "")
	HOSTNAME_SMARTSELF := cfg.GetString("HOSTNAME_SMARTSELF", "")
	DBNAME_SMARTSELF := cfg.GetString("DBNAME_SMARTSELF", "")

	Username := USERNAME_SMARTSELF
	Password := PASSWORD_SMARTSELF
	Hostname := HOSTNAME_SMARTSELF
	Dbname := DBNAME_SMARTSELF
	return Username, Password, Hostname, Dbname
}

func LoadPathSaveImages() string {
	cfg := properties.MustLoadFile(ADDRESS, properties.UTF8)
	Path := cfg.GetString("SAVE_IMG_PATH", "")
	return Path
}
