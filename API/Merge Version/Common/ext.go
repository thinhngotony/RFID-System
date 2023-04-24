package Common

import (
	"os"
	"regexp"
	"sort"
	"strings"
	"time"
)

func SQLPara(arrStr []string) string {
	return strings.Trim(strings.Repeat("?,", len(arrStr)), ",")
}

func ToInterfaceArray(arrStr []string) []interface{} {
	b := make([]interface{}, len(arrStr))
	for i := range arrStr {
		b[i] = arrStr[i]
	}
	return b
}

func MakeDir(path string) error {
	if _, err := os.Stat(path); os.IsNotExist(err) {
		return os.MkdirAll(path, 0777)
	}
	return nil
}

const (
	DATE_FORMAT_YMD        = "20060102"
	DATE_FORMAT_YMD_HYPHEN = "2006-01-02"
	DATE_FORMAT_YMDHMS     = "20060102150405"
	TIMSESTAMP_FORMAT      = "2006-01-02 15:04:05.000"
	DATE_FORMAT_YYMM       = "0601"
	DATE_FORMAT_MMDD       = "0102"
)

// yyyymmdd
func CurrentDate() string {
	return time.Now().Format(DATE_FORMAT_YMD)
}

// yyyy-mm-dd
func CurrentDateHyphen() string {
	return time.Now().Format(DATE_FORMAT_YMD_HYPHEN)
}

//DayFromToday date of (today + n day) in format yyyymmdd
func DayFromToday(day int) string {
	now := time.Now().AddDate(0, 0, day)
	return now.Format(DATE_FORMAT_YMD)
}

// yyyymmddhhMMss
func CurrentDateTime() string {
	return time.Now().Format(DATE_FORMAT_YMDHMS)
}

//yymm
func CurrentYYMM() string {
	return time.Now().Format(DATE_FORMAT_YYMM)
}

//mmdd
func CurrentMMDD() string {
	return time.Now().Format(DATE_FORMAT_MMDD)
}

func CurrentTimeStamp() string {
	return time.Now().Format(TIMSESTAMP_FORMAT)
}

//DateAddMonth add nMonth to yyyymmdd
func DateAddMonth(yyyymmdd string, nMonth int) string {
	t, err := time.Parse(DATE_FORMAT_YMD, yyyymmdd)
	if err != nil {
		return yyyymmdd
	}
	return t.AddDate(0, nMonth, 0).Format(DATE_FORMAT_YMD)
}

//DateAddHyphen convert yyyymmdd to yyyy-mm-dd
func DateAddHyphen(yyyymmdd string) string {
	if matched, _ := regexp.MatchString("^\\d{8}$", yyyymmdd); matched {
		return_year := yyyymmdd[0:4]
		return_month := yyyymmdd[4:6]
		return_day := yyyymmdd[6:8]
		return_date := return_year + "-" + return_month + "-" + return_day
		return return_date
	}
	return yyyymmdd
}

//DateAddDay add nDate to yyyymmdd
func DateAddDay(yyyymmdd string, nDay int) string {
	t, err := time.Parse(DATE_FORMAT_YMD, yyyymmdd)
	if err != nil {
		return yyyymmdd
	}
	return t.AddDate(0, 0, nDay).Format(DATE_FORMAT_YMD)
}

func MapBoolToListKey(m map[string]bool) []string {
	var keys []string
	for k := range m {
		keys = append(keys, k)
	}

	sort.Strings(keys)
	return keys
}

func Split1ServerShop(serverShop string) (server string, shop string) {
	a := strings.Split(serverShop, "|")
	if len(a) == 2 {
		return a[0], a[1]
	}
	return serverShop, ""
}

func SplitServerShop(serverShop []string) (server []string, shop []string) {
	serverMap := map[string]bool{}
	shopMap := map[string]bool{}
	for _, s := range serverShop {
		a := strings.Split(s, "|")
		if len(a) == 2 {
			serverMap[a[0]] = true
			shopMap[a[1]] = true
		}
	}

	return MapBoolToListKey(serverMap), MapBoolToListKey(shopMap)
}

//DayFromToday date of (today + n day) in format yyyy-mm-dd
func DayFromTodayHyphen(day int) string {
	date_format := DATE_FORMAT_YMD_HYPHEN
	now := time.Now().AddDate(0, 0, day)
	return now.Format(date_format)
}

//MakeListDate make list date in format yyyymmdd
func MakeListDate(from, to string) []string {
	var ls []string

	tFrom, err := time.Parse(DATE_FORMAT_YMD, from)
	if err != nil {
		return ls
	}
	tTo, err := time.Parse(DATE_FORMAT_YMD, to)
	if err != nil {
		return ls
	}

	for ; !tFrom.After(tTo); tFrom = tFrom.AddDate(0, 0, 1) {
		ls = append(ls, tFrom.Format(DATE_FORMAT_YMD))
	}

	return ls
}

func RoundUp(x float64) int64 {
	return int64(x + 1.0)
}
