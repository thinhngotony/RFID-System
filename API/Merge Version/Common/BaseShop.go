package Common

import (
	"database/sql"
	"log"
	"strings"

	"github.com/goframework/gf/db"
	"github.com/goframework/gf/exterror"
)

const FRANCHISE_GROUP_PREFIX = "G-"

func MakeFullBaseShopList(shopList []string, franchiseGroupList []string) []string {
	selectedShopCdBase := []string{}
	mapSelectedShopCdBase := map[string]bool{}
	for _, shopCd := range shopList {
		selectedShopCdBase = append(selectedShopCdBase, shopCd)
		mapSelectedShopCdBase[shopCd] = true
	}

	franchiseGroup := []string{}
	for _, f := range franchiseGroupList {
		if strings.HasPrefix(f, FRANCHISE_GROUP_PREFIX) {
			f = f[len(FRANCHISE_GROUP_PREFIX):]
			franchiseGroup = append(franchiseGroup, f)
		}
	}

	dbu, err := GetDBUtil()
	if err != nil {
		log.Println(exterror.WrapExtError(err))
		return nil
	}

	if len(franchiseGroup) > 0 {
		shm := ShopMasterModel{dbu.DB}

		specialGroupShops, err := shm.GetListShopByFranchiseGroups(franchiseGroup)
		if err != nil {
			log.Println(exterror.WrapExtError(err))
			return nil
		}

		for _, sh := range specialGroupShops {
			shopKey := sh.ServerName + "|" + sh.ShopCD
			if !mapSelectedShopCdBase[shopKey] {
				selectedShopCdBase = append(selectedShopCdBase, shopKey)
				mapSelectedShopCdBase[shopKey] = true
			}
		}
	}

	return selectedShopCdBase
}

type ShopMasterModel struct {
	DB *sql.DB
}

type ShopItem struct {
	ShopCD     string `sql:"shm_shop_cd"`
	ServerName string `sql:"shm_server_name"`
}

func (this *ShopMasterModel) GetListShopByFranchiseGroups(frGroups []string) ([]ShopItem, error) {
	query := `
SELECT
	shm_server_name,
    shm_shop_cd
FROM
	(SELECT
		sfgm_franchise_cd
	FROM shop_franchise_group_master
	WHERE sfgm_franchise_group_cd IN (` + SQLPara(frGroups) + `)) fcd
JOIN shop_master_show shm
ON sfgm_franchise_cd = shm_franchise_cd
	`

	rows, err := this.DB.Query(query, ToInterfaceArray(frGroups)...)
	if rows != nil {
		defer rows.Close()
	}
	if err != nil {
		return nil, exterror.WrapExtError(err)
	}

	listShop := []ShopItem{}
	for rows.Next() {
		newShopItem := ShopItem{}
		err = db.SqlScanStruct(rows, &newShopItem)
		if err != nil {
			return nil, exterror.WrapExtError(err)
		}
		listShop = append(listShop, newShopItem)
	}
	return listShop, nil
}
