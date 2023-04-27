package Common

import (
	"fmt"
	"strings"

	"ProductManage/github.com/goframework/gcp/bq"

	"github.com/goframework/gf/exterror"
)

type WorkTypeName int

const (
	WTOrder WorkTypeName = iota
	WTSale
	WTSupply
	WTReturn
	WTMove
)

var mapWorkType = map[WorkTypeName][]string{

	WTOrder: {
		"10000002", //イニシャル発注
		"10000003", //本部発注
		"10000004", //ＤＩＳＫ発注
		"10000005", //書籍発注
		"10000006", //定期改正
		"10000007", //ゲーム発注
		"10000008", //文具発注
		"10000009", //客注
		"10000010", //取置取消
		"10000011", //自動発注
		"10000012", //補充発注
		"10000013", //常備発注
		"10000014", //発注回答
		"10000015", //定期改定
		"10000016", //発注取消	-
		"10000017", //メディア注文
	},
	WTSale: {
		"00000000", //売上
		"00000001", //売上返品   -
		"00000002", //取置売上
	},
	WTSupply: {
		"20000008", //仕入
		"20000009", //直接仕入
		"20000012", //仕入本伝確定
	},
	WTReturn: {
		"30000006", //本部返品
		"30000007", //書籍・雑誌返品
		"30000008", //書籍返品
		"30000009", //雑誌返品
		"30000010", //DISK返品
		"30000011", //直接返品
		"30000012", //長期返品
		"30000013", //常備返品
		"30000014", //地方返品
		"30000015", //その他返品
		"30000016", //丸投げ返品（在庫更新）
		"30000017", //丸投げ返品
	},
	WTMove: {
		"21000005", //移動入庫
		"21000006", //移動出庫
	},
}

var mapWorkTypeMinus = map[WorkTypeName][]string{
	WTOrder: {
		"10000016", //発注取消	-
	},
	WTSale: {
		"00000001", //売上返品   -
	},
}

func GetBQWorkType(workTypeName []WorkTypeName) []string {
	var wtList []string
	for _, wt := range workTypeName {
		wtList = append(wtList, mapWorkType[wt]...)
	}
	return wtList
}

func GetBQWorkTypeStockIn() []string {
	return []string{
		"00000001",
		"20000008",
		"20000009",
		"20000012",
		"21000005",
	}
}
func CreateStdFieldBQInoutCountStockout() string {
	stockInWT := GetBQWorkTypeStockIn()
	const bqioGoodsCount = "bqio_goods_count"

	if len(stockInWT) > 0 {
		cmd := bq.NewCommand()
		cmd.CommandText = `IF(bqio_bq_work_type IN {{stockInWT}},-` + bqioGoodsCount + `,` + bqioGoodsCount + `)`
		cmd.Parameters["stockInWT"] = stockInWT
		query, err := cmd.Build()
		LogError(&ErrorDetail{exterror.WrapExtError(err), ERR_QUERY_BUILD})
		return query
	}
	return bqioGoodsCount
}

func GetBQWorkTypeMinus(workTypeName []WorkTypeName) []string {
	var wtList []string
	for _, wt := range workTypeName {
		wtList = append(wtList, mapWorkTypeMinus[wt]...)
	}
	return wtList
}

func CreateStdFieldBQInoutCount(workTypeName []WorkTypeName) string {
	minusWT := GetBQWorkTypeMinus(workTypeName)
	const bqioGoodsCount = "bqio_goods_count"

	if len(minusWT) > 0 {
		cmd := bq.NewCommand()
		cmd.CommandText = `IF(bqio_bq_work_type IN {{minusWT}},-` + bqioGoodsCount + `,` + bqioGoodsCount + `)`
		cmd.Parameters["minusWT"] = minusWT
		query, err := cmd.Build()
		LogError(&ErrorDetail{exterror.WrapExtError(err), ERR_QUERY_BUILD})
		return query
	}
	return bqioGoodsCount
}

func CreateStdQueryBQInout(dateFrom, dateTo string, serverShopList []string, bqWorkTypeNameList []WorkTypeName, selectFields []string, groupFirstFields int, andWhereStms []string) string {
	cmd := bq.NewCommand()

	var whereStms []string

	cmd.CommandText = `
SELECT 
` + strings.Join(selectFields, ",\n") + `
FROM WPC_Data.bq_inout
WHERE
`
	/// TEST : getting data from ioStock
	/*if dateFrom != "" {
		whereStms = append(whereStms, `bqio_partition_date < "`+DateAddHyphen(dateFrom)+`"`)
	}
	if dateTo != "" {
		whereStms = append(whereStms, `bqio_partition_date > "`+DateAddHyphen(dateTo)+`"`)
	}*/

	if len(serverShopList) > 0 {
		servers, shops := SplitServerShop(serverShopList)
		whereStms = append(whereStms, `
bqio_servername IN {{servers}}
AND bqio_shop_cd IN {{shops}}
AND CONCAT(bqio_servername,"|",bqio_shop_cd) IN {{serverShopList}}`)
		cmd.Parameters["servers"] = servers
		cmd.Parameters["shops"] = shops
		cmd.Parameters["serverShopList"] = serverShopList
	}

	if bqWorkTypeList := GetBQWorkType(bqWorkTypeNameList); len(bqWorkTypeList) > 0 {
		whereStms = append(whereStms, `bqio_bq_work_type IN {{bqWorkTypeList}}`)
		cmd.Parameters["bqWorkTypeList"] = bqWorkTypeList
	}

	whereStms = append(whereStms, andWhereStms...)

	cmd.CommandText += strings.Join(whereStms, "\nAND ")

	if groupFirstFields > 0 {
		var groupNum []string
		for i := 1; i <= groupFirstFields; i++ {
			groupNum = append(groupNum, fmt.Sprint(i))
		}
		cmd.CommandText += `
GROUP BY ` + strings.Join(groupNum, ",")

	}

	query, err := cmd.Build()
	LogError(&ErrorDetail{exterror.WrapExtError(err), ERR_QUERY_BUILD})
	return query
}
func CreateStdQueryBQInout_withDate(dateFrom, dateTo string, serverShopList []string, bqWorkTypeNameList []WorkTypeName, selectFields []string, groupFirstFields int, andWhereStms []string) string {
	cmd := bq.NewCommand()

	var whereStms []string

	cmd.CommandText = `
SELECT 
` + strings.Join(selectFields, ",\n") + `
FROM WPC_Data.bq_inout
WHERE
`

	if dateFrom != "" {
		whereStms = append(whereStms, `bqio_partition_date >= "`+DateAddHyphen(dateFrom)+`"`)
	}
	if dateTo != "" {
		whereStms = append(whereStms, `bqio_partition_date <= "`+DateAddHyphen(dateTo)+`"`)
	}

	if len(serverShopList) > 0 {
		servers, shops := SplitServerShop(serverShopList)
		whereStms = append(whereStms, `
bqio_servername IN {{servers}}
AND bqio_shop_cd IN {{shops}}
AND CONCAT(bqio_servername,"|",bqio_shop_cd) IN {{serverShopList}}`)
		cmd.Parameters["servers"] = servers
		cmd.Parameters["shops"] = shops
		cmd.Parameters["serverShopList"] = serverShopList
	}

	if bqWorkTypeList := GetBQWorkType(bqWorkTypeNameList); len(bqWorkTypeList) > 0 {
		whereStms = append(whereStms, `bqio_bq_work_type IN {{bqWorkTypeList}}`)
		cmd.Parameters["bqWorkTypeList"] = bqWorkTypeList
	}

	whereStms = append(whereStms, andWhereStms...)

	cmd.CommandText += strings.Join(whereStms, "\nAND ")

	if groupFirstFields > 0 {
		var groupNum []string
		for i := 1; i <= groupFirstFields; i++ {
			groupNum = append(groupNum, fmt.Sprint(i))
		}
		cmd.CommandText += `
GROUP BY ` + strings.Join(groupNum, ",")

	}

	query, err := cmd.Build()
	LogError(&ErrorDetail{exterror.WrapExtError(err), ERR_QUERY_BUILD})
	return query
}

func CreateStdQueryStock(date string, serverShops []string) string {
	date = strings.Replace(strings.Replace(date, "-", "", -1), "/", "", -1)
	var ioDateTo string
	var stockTable string
	var stockPartitionDateCond string

	ioDateFrom := DateAddDay(date, 1)
	if strings.HasPrefix(CurrentDate(), date[:6]) {
		ioDateTo = DayFromToday(-1)
		stockTable = "WPC_Data.bq_stok_cur"
	} else {
		ioDateTo = DateAddDay(DateAddMonth(date[:6]+"01", 1), -1)
		stockTable = "WPC_Data.bq_stok_ym"
		stockPartitionDateCond = "AND bqsc_partition_date = '" + DateAddHyphen(date[:6]+"01") + "'"
	}

	cmd := bq.NewCommand()
	cmd.CommandText = `
---------------------------------------
ioStockOut AS (` +
		// TEST:
		CreateStdQueryBQInout_withDate(ioDateFrom, ioDateTo, serverShops, []WorkTypeName{WTSale, WTSupply, WTReturn, WTMove}, []string{
			"bqio_servername",
			"bqio_shop_cd",
			"bqio_jan_cd",
			"SUM(" + CreateStdFieldBQInoutCountStockout() + ") bqio_goods_count",
		}, 3, nil) + `),
---------------------------------------
stockBase AS (
SELECT
  bqsc_servername,
  bqsc_shop_cd,
  bqsc_jan_cd,
  bqsc_stock_count
FROM ` + stockTable + `
WHERE
  bqsc_servername IN {{bqsc_servername}} 
  AND bqsc_shop_cd IN {{bqsc_shop_cd}}
  AND CONCAT(bqsc_servername, '|', bqsc_shop_cd) IN {{serverShops}}
  ` + stockPartitionDateCond + `),
---------------------------------------
stock AS (SELECT
  IFNULL(bqsc_servername, bqio_servername) bqsc_servername,
  IFNULL(bqsc_shop_cd, bqio_shop_cd) bqsc_shop_cd,
  IFNULL(bqsc_jan_cd, bqio_jan_cd) bqsc_jan_cd,
  IFNULL(bqsc_stock_count,0) - IFNULL(bqio_goods_count,0) bqsc_stock_count
FROM stockBase
FULL JOIN ioStockOut
ON 
  bqsc_servername = bqio_servername
  AND bqsc_shop_cd = bqio_shop_cd
  AND bqsc_jan_cd = bqio_jan_cd)
`

	cmd.Parameters["bqsc_servername"], cmd.Parameters["bqsc_shop_cd"] = SplitServerShop(serverShops)
	cmd.Parameters["serverShops"] = serverShops

	query, err := cmd.Build()
	LogError(&ErrorDetail{exterror.WrapExtError(err), ERR_QUERY_BUILD})

	return query
}

func CreateStdQueryGoodsMaster(cond *GoodsSearchCondition) string {
	cmd := bq.NewCommand()
	cmd.CommandText = `
SELECT
	bqgm_jan_cd,
	bqgm_price,
	SUBSTR(bqgm_media_cd, 1, 4) bqgm_media_group3_cd
FROM {{@src_dataset}}.bq_goods_master
`
	if len(cond.MediaCd1) > 0 {
		cmd.CommandText += `
JOIN (
	SELECT
		bqct_media_group2_cd
	FROM {{@src_dataset}}.bq_category_ms
	WHERE
		bqct_media_group2_cd IS NOT NULL
		AND bqct_media_group1_cd IN {{MediaCd1}}
	GROUP BY 1
)
ON
	SUBSTR(bqgm_media_cd, 1, 2) = bqct_media_group2_cd
`
		cmd.Parameters["MediaCd1"] = cond.MediaCd1
	}

	var whereList []string
	if len(cond.MediaCd2) > 0 {
		whereList = append(whereList, `SUBSTR(bqgm_media_cd, 1, 2) IN {{MediaCd2}}`)
		cmd.Parameters["MediaCd2"] = cond.MediaCd2
	}
	if len(cond.MediaCd3) > 0 {
		whereList = append(whereList, `SUBSTR(bqgm_media_cd, 1, 4) IN {{MediaCd3}}`)
		cmd.Parameters["MediaCd3"] = cond.MediaCd3
	}
	if len(cond.MediaCd4) > 0 {
		whereList = append(whereList, `SUBSTR(bqgm_media_cd, 1, 6) IN {{MediaCd4}}`)
		cmd.Parameters["MediaCd4"] = cond.MediaCd4
	}
	if cond.SaleDateFrom != 0 {
		whereList = append(whereList, `bqgm_sales_date >= {{SaleDateFrom}}`)
		cmd.Parameters["SaleDateFrom"] = DayFromToday(int(cond.SaleDateFrom))
	}
	if cond.SaleDateTo != 0 {
		whereList = append(whereList, `bqgm_sales_date <= {{SaleDateTo}}`)
		cmd.Parameters["SaleDateTo"] = DayFromToday(int(cond.SaleDateTo))
	}
	if cond.PriceFrom > 0 {
		whereList = append(whereList, `bqgm_price >= {{PriceFrom}}`)
		cmd.Parameters["PriceFrom"] = cond.PriceFrom
	}
	if cond.PriceTo > 0 {
		whereList = append(whereList, `bqgm_price >= {{PriceTo}}`)
		cmd.Parameters["PriceTo"] = cond.PriceTo
	}
	if cond.Writer != "" {
		whereList = append(whereList, `bqgm_writer_name LIKE {{Writer}}`)
		cmd.Parameters["Writer"] = "%" + cond.Writer + "%"
	}
	if cond.Publisher != "" {
		if strings.HasPrefix(cond.Publisher, "9784") {
			whereList = append(whereList, `bqgm_publisher_cd LIKE {{publisher_cd}}`)
			cmd.Parameters["publisher_cd"] = cond.Publisher + "%"
		} else {
			whereList = append(whereList, `bqgm_publisher_name LIKE {{publisher_name}}`)
			cmd.Parameters["publisher_name"] = "%" + cond.Publisher + "%"
		}
	}
	if len(whereList) > 0 {
		cmd.CommandText += `
WHERE ` + strings.Join(whereList, "\nAND ")
	}

	cmd.Parameters["@src_dataset"] = GConfig.StrOrEmpty(CfgBQSrcDataset)

	query, err := cmd.Build()
	LogError(&ErrorDetail{exterror.WrapExtError(err), ERR_QUERY_BUILD})

	return query
}
