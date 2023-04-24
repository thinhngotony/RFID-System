package db_client

import (
	"context"
	"database/sql"
	"fmt"
	"log"
	"main/utils"
	"strconv"
	"time"

	_ "github.com/go-sql-driver/mysql"
)

type Set_SmartSelf_Setting struct {
	ApiKey            string `json:"api_key" validate:"required"`
	Dpp_shelf_pos     int    `json:"dpp_shelf_pos"`
	Dpp_shelf_col_pos int    `json:"dpp_shelf_col_pos"`
	Dpp_jan_cd        string `json:"dpp_jan_cd"`
	Dpp_rfid_cd       string `json:"dpp_rfid_cd"`
	Dpp_isbn          string `json:"dpp_isbn"`
	Dpp_product_name  string `json:"dpp_product_name"`
	Dpp_scaner_name   string `json:"dpp_scaner_name"`
	Dpp_shelf_name    string `json:"dpp_shelf_name"`
	Dpp_image_url     string `json:"dpp_image_url"`
}

type Get_SmartSelf_Setting struct {
	ApiKey            *string `json:"api_key" validate:"required"`
	Dpp_shelf_pos     *int    `json:"dpp_shelf_pos"`
	Dpp_shelf_col_pos *int    `json:"dpp_shelf_col_pos"`
	Dpp_jan_cd        *string `json:"dpp_jan_cd"`
	Dpp_rfid_cd       *string `json:"dpp_rfid_cd"`
	Dpp_isbn          *string `json:"dpp_isbn"`
	Dpp_product_name  *string `json:"dpp_product_name"`
	Dpp_scaner_name   *string `json:"dpp_scaner_name"`
	Dpp_shelf_name    *string `json:"dpp_shelf_name"`
	Dpp_image_url     *string `json:"dpp_image_url"`
}

type SmartSelf_Position struct {
	Dpp_shelf_pos     int    `json:"dpp_shelf_pos"`
	Dpp_shelf_col_pos int    `json:"dpp_shelf_col_pos"`
	Dpp_rfid_cd       string `json:"dpp_rfid_cd"`
	Dpp_shelf_name    string `json:"dpp_shelf_name"`
}

type Drfid_log_move struct {
	Dlm_id      *string `json:"dlm_id"`
	Dlm_date    *string `json:"dlm_date"`
	Dlm_rfid_cd *string `json:"dlm_rfid_cd"`
	Dlm_cnt     *string `json:"dlm_cnt"`
	Dlm_outdate *string `json:"dlm_outdate"`
	Dlm_indate  *string `json:"dlm_indate"`
}

type Get_SmartSelf_Location struct {
	ApiKey       string  `json:"api_key" validate:"required"`
	Shelf_no     string  `json:"shelf_no"`
	EPC          string  `json:"EPC"`
	Jancode      *string `json:"jancode"`
	Product_name *string `json:"product_name"`
	Link_image   *string `json:"link_image"`
	Col          int     `json:"col"`
	Row          int     `json:"row"`
}

type Get_SmartSelf_Location_2 struct {
	ApiKey       string `json:"api_key" validate:"required"`
	Shelf_no     string `json:"shelf_no"`
	EPC          string `json:"EPC"`
	Jancode      string `json:"jancode"`
	Product_name string `json:"product_name"`
	Link_image   string `json:"link_image"`
	Col          int    `json:"col"`
	Row          int    `json:"row"`
}

type Set_SmartShelf_Position_mst_antena struct {
	ApiKey         string `json:"api_key" validate:"required"`
	Shelf_no       string `json:"shelf_no" validate:"required"`
	Antena_no      string `json:"antena_no"`
	Antena_index   string `json:"antena_index"`
	Direction      string `json:"direction"`
	Row            int    `json:"row"`
	Col            int    `json:"col"`
	ColSize        int    `json:"col_size"`
	M_min          int    `json:"m_min"`
	M_max          int    `json:"m_max"`
	Scan_col_start int    `json:"scan_col_start" validate:"required"`
	Scan_col_end   int    `json:"scan_col_end"`
}

type Get_SmartShelf_Position_mst_antena struct {
	ApiKey         string  `json:"api_key" validate:"required"`
	Shelf_no       *string `json:"shelf_no" validate:"required"`
	Antena_no      *string `json:"antena_no"`
	Antena_index   *string `json:"antena_index"`
	Direction      *string `json:"direction"`
	Row            *int    `json:"row"`
	Col            *int    `json:"col"`
	ColSize        *int    `json:"col_size"`
	M_min          *int    `json:"m_min"`
	M_max          *int    `json:"m_max"`
	Scan_col_start *int    `json:"scan_col_start"`
	Scan_col_end   *int    `json:"scan_col_end"`
}

type Clear_SmartShelf_Position_mst_antena struct {
	ApiKey   string `json:"api_key" validate:"required"`
	Shelf_no string `json:"shelf_no" validate:"required"`
}

func DbConnection_smartself() (*sql.DB, error) {
	Username, Password, Hostname, Dbname := utils.LoadDatabase_SmartSelf()
	db, err := sql.Open("mysql", fmt.Sprintf("%s:%s@tcp(%s)/%s", Username, Password, Hostname, Dbname))
	if err != nil {
		log.Printf("Error %s when opening DB\n", err)
		return nil, err
	}
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	err = db.PingContext(ctx)
	if err != nil {
		log.Printf("Errors %s pinging database", err)
		return nil, err
	}
	log.Printf("Verified connection from database with Ping\n")
	return db, nil
}
func GetSmartSelfSetting(db *sql.DB, shelf_name string) ([]Get_SmartSelf_Setting, bool, error) {
	log.Printf("Getting setting of smart self")
	query := `select 
	dpp_shelf_pos,
	dpp_shelf_col_pos, 
	dpp_jan_cd, 
	dpp_rfid_cd, 
	dpp_isbn, 
	dpp_product_name, 
	dpp_scaner_name, 
	dpp_shelf_name,
	dpp_image_url
	from drfid_product_pos
	WHERE dpp_shelf_name = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, false, err

	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, shelf_name)
	if err != nil {
		log.Printf("Error %s when query SQL statement", err)
		return nil, false, err
	}

	defer rows.Close()

	var data_list = []Get_SmartSelf_Setting{}

	for rows.Next() {
		var data Get_SmartSelf_Setting
		if err := rows.Scan(&data.Dpp_shelf_pos,
			&data.Dpp_shelf_col_pos,
			&data.Dpp_jan_cd,
			&data.Dpp_rfid_cd,
			&data.Dpp_isbn,
			&data.Dpp_product_name,
			&data.Dpp_scaner_name,
			&data.Dpp_shelf_name,
			&data.Dpp_image_url); err != nil {
			log.Println(err)
			return []Get_SmartSelf_Setting{}, false, err
		}
		data_list = append(data_list, data)
	}

	if err := rows.Err(); err != nil {
		return data_list, false, err
	}
	if len(data_list) == 0 {
		return nil, false, err
	}

	return data_list, true, nil

}

func ShelfExists(db *sql.DB, shelf_name string) bool {
	query := `SELECT dpp_shelf_name FROM drfid_product_pos WHERE dpp_shelf_name = ?`
	err := db.QueryRow(query, shelf_name).Scan(&shelf_name)
	if err != nil {
		if err != sql.ErrNoRows {
			log.Print(err)
		}
		return false
	}
	return true
}

func GetShelfNames(db *sql.DB) ([]string, error) {
	query := `SELECT dpp_shelf_name FROM drfid_product_pos GROUP BY dpp_shelf_name`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, err

	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx)
	if err != nil {
		log.Printf("Error %s when query SQL statement", err)
		return nil, err
	}

	defer rows.Close()

	var shelf_names_result []string

	for rows.Next() {
		var data string
		if err := rows.Scan(&data); err != nil {
			log.Println(err)
			return nil, err
		}
		shelf_names_result = append(shelf_names_result, data)
	}

	if err := rows.Err(); err != nil {
		return nil, err
	}

	return shelf_names_result, nil
}

func GetShelfNamesMSTAntena(db *sql.DB) ([]string, error) {
	query := `SELECT shelf_no FROM mst_antena GROUP BY shelf_no`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, err

	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx)
	if err != nil {
		log.Printf("Error %s when query SQL statement", err)
		return nil, err
	}

	defer rows.Close()

	var shelf_names_result []string

	for rows.Next() {
		var data string
		if err := rows.Scan(&data); err != nil {
			log.Println(err)
			return nil, err
		}
		shelf_names_result = append(shelf_names_result, data)
	}

	if err := rows.Err(); err != nil {
		return nil, err
	}

	return shelf_names_result, nil
}

func GetRowInShelfMSTAntena(db *sql.DB, shelf_no string) ([]string, error) {
	query := `SELECT antena_no FROM mst_antena where shelf_no = ? GROUP BY antena_no`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, err

	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, shelf_no)
	if err != nil {
		log.Printf("Error %s when query SQL statement", err)
		return nil, err
	}

	defer rows.Close()

	var shelf_rows_result []string

	for rows.Next() {
		var data string
		if err := rows.Scan(&data); err != nil {
			log.Println(err)
			return nil, err
		}
		shelf_rows_result = append(shelf_rows_result, data)
	}

	if err := rows.Err(); err != nil {
		return nil, err
	}
	return shelf_rows_result, nil
}

func CheckExistKeyInMST(db *sql.DB, shelf_no string, antena_no string) (bool, error) {
	query := `SELECT count(*) FROM mst_antena where shelf_no = ? and antena_no = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, err
	}
	defer stmt.Close()
	var count int
	row := stmt.QueryRowContext(ctx, shelf_no, antena_no)
	if err := row.Scan(&count); err != nil {

		return false, err
	}
	if count > 0 {
		return true, nil
	}

	return false, nil
}

func LoadPositionMSTAntena(db *sql.DB, shelf_no string) ([]Get_SmartShelf_Position_mst_antena, error) {
	log.Printf("Loading position for MST Antena")
	query := `SELECT shelf_no, antena_no, antena_index, direction, row, col, col_size, m_min, m_max, scan_col_start, scan_col_end FROM mst_antena where shelf_no = ?`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		fmt.Printf("Error %s when preparing SQL statement", err)
		return []Get_SmartShelf_Position_mst_antena{}, err

	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, shelf_no)
	if err != nil {
		fmt.Printf("Error %s when query SQL statement", err)
		return []Get_SmartShelf_Position_mst_antena{}, err

	}
	defer rows.Close()

	var data_list = []Get_SmartShelf_Position_mst_antena{}

	for rows.Next() {
		var data Get_SmartShelf_Position_mst_antena
		if err := rows.Scan(&data.Shelf_no,
			&data.Antena_no,
			&data.Antena_index,
			&data.Direction,
			&data.Row,
			&data.Col,
			&data.ColSize,
			&data.M_min,
			&data.M_max,
			&data.Scan_col_start,
			&data.Scan_col_end); err != nil {
			fmt.Println(err)
			return []Get_SmartShelf_Position_mst_antena{}, err
		}
		data_list = append(data_list, data)
	}

	if err := rows.Err(); err != nil {
		return data_list, err
	}

	return data_list, nil

}

func CheckExistPosition(db *sql.DB, position Set_SmartSelf_Setting) (bool, error) {
	log.Printf("Checking position in smartself database")
	query := `select dpp_shelf_pos, dpp_shelf_col_pos from drfid_product_pos where dpp_shelf_name = ? && dpp_shelf_pos = ? && dpp_shelf_pos && dpp_shelf_col_pos = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, err
	}
	defer stmt.Close()
	var data SmartSelf_Position
	row := stmt.QueryRowContext(ctx, position.Dpp_shelf_name, position.Dpp_shelf_pos, position.Dpp_shelf_col_pos)
	if err := row.Scan(&data.Dpp_shelf_pos, &data.Dpp_shelf_col_pos); err != nil {

		return false, err
	}

	return true, nil
}

func CheckExistRFID_SmartSelf(db *sql.DB, rfid string, force_update bool) (bool, error) {
	log.Printf("Checking RFID in smartself database")
	query := `SELECT dpp_rfid_cd FROM drfid_product_pos WHERE dpp_rfid_cd = ?`
	err := db.QueryRow(query, rfid).Scan(&rfid)
	if err != nil {
		if err != sql.ErrNoRows {
			log.Print(err)
		}
	}

	if force_update {
		if rfid != "" {
			query := `DELETE FROM drfid_product_pos WHERE dpp_rfid_cd = ?`
			ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
			defer cancelfunc()

			tx, err := db.BeginTx(ctx, nil)
			if err != nil {
				log.Fatal(err)
			}

			_, err = tx.ExecContext(ctx, query, rfid)
			if err != nil {
				// Incase we find any error in the query execution, rollback the transaction
				log.Printf("Error %s when finding rows affected", err)
				tx.Rollback()
				return false, err
			}
			err = tx.Commit()
			if err != nil {
				log.Printf("Error %s when finding rows affected", err)
				return false, err
			}

			return false, err
		} else {
			return false, nil
		}
	}
	return true, nil
}

func InsertSmartSelfSetting(db *sql.DB, reqBody Set_SmartSelf_Setting) (bool, error) {
	log.Printf("Importing data to drfid_product_pos table")
	query := `INSERT INTO drfid_product_pos
	(dpp_shelf_pos,
	dpp_shelf_col_pos, 
	dpp_jan_cd, 
	dpp_rfid_cd, 
	dpp_isbn, 
	dpp_product_name, 
	dpp_scaner_name, 
	dpp_shelf_name,
	dpp_image_url) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, err
	}
	defer stmt.Close()
	res, err := stmt.ExecContext(ctx, reqBody.Dpp_shelf_pos,
		reqBody.Dpp_shelf_col_pos,
		reqBody.Dpp_jan_cd,
		reqBody.Dpp_rfid_cd,
		reqBody.Dpp_isbn,
		reqBody.Dpp_product_name,
		reqBody.Dpp_scaner_name,
		reqBody.Dpp_shelf_name,
		reqBody.Dpp_image_url)
	if err != nil {
		fmt.Printf("Error %s when inserting row into log table", err)
		return false, err
	}
	rows, err := res.RowsAffected()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}
	log.Printf("%d rows inserted ", rows)
	return true, err
}

func UpdateSmartSelfSetting(db *sql.DB, reqBody Set_SmartSelf_Setting) (bool, error) {
	log.Printf("Updating data to drfid_product_pos table")
	query := `UPDATE drfid_product_pos SET
	dpp_jan_cd = ?, 
	dpp_rfid_cd = ?, 
	dpp_isbn = ?, 
	dpp_product_name = ?, 
	dpp_scaner_name = ?,
	dpp_image_url = ?
	WHERE dpp_shelf_pos = ? AND dpp_shelf_col_pos = ? AND dpp_shelf_name = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, err
	}
	defer stmt.Close()
	res, err := stmt.ExecContext(ctx,
		reqBody.Dpp_jan_cd,
		reqBody.Dpp_rfid_cd,
		reqBody.Dpp_isbn,
		reqBody.Dpp_product_name,
		reqBody.Dpp_scaner_name,
		reqBody.Dpp_image_url,
		reqBody.Dpp_shelf_pos,
		reqBody.Dpp_shelf_col_pos,
		reqBody.Dpp_shelf_name)
	if err != nil {
		log.Printf("Error %s when updating rows into smartself database", err)
		return false, err
	}
	rows, err := res.RowsAffected()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}
	log.Printf("%d rows updated ", rows)
	return true, err
}

func CheckStatusRFID_SmartSelf(db *sql.DB, rfid string) (Drfid_log_move, bool, error) {
	log.Printf("Checking status RFID in smartself database")
	query := `SELECT * FROM smart_shelf.drfid_log_move where dlm_rfid_cd = ? order by dlm_cnt desc limit 1 ;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return Drfid_log_move{}, false, err

	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, rfid)
	if err != nil {
		log.Printf("Error %s when query SQL statement", err)

		return Drfid_log_move{}, false, err
	}

	defer rows.Close()

	var data = Drfid_log_move{}

	for rows.Next() {
		if err := rows.Scan(&data.Dlm_id,
			&data.Dlm_date,
			&data.Dlm_rfid_cd,
			&data.Dlm_cnt,
			&data.Dlm_outdate,
			&data.Dlm_indate); err != nil {
			return Drfid_log_move{}, false, err
		}
	}
	if err := rows.Err(); err != nil {
		return data, false, err
	}

	if data.Dlm_rfid_cd == nil {
		return data, false, nil
	} else if data.Dlm_indate == nil {
		return data, false, nil
	} else {
		return data, true, nil
	}
}

func ClearSmartSelfLocationRAW(db *sql.DB) (bool, error) {
	query := `TRUNCATE TABLE drfid_raw_data;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, query)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, nil
}

func ClearSmartSelfLocation(db *sql.DB) (bool, error) {
	query := `TRUNCATE TABLE shelf_calc_location;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, query)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func ConvertToDecimalSmartSelfLocation(db *sql.DB) (bool, error) {
	query := `UPDATE drfid_raw_data 
	SET m_10 = CONV( drd_rssi, 16, 10 ),
	n_10 = CONV( drd_anten_no, 2, 10 );`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, query)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func SetAverageValueForRSSI(db *sql.DB) (bool, error) {
	query := `UPDATE drfid_raw_data sl
	SET 
		m_10 = (SELECT 
				sl_avg.m_avg
			FROM
				(SELECT 
					AVG(m_10) m_avg, drd_rfid_cd, drd_anten_no
				FROM
					drfid_raw_data sl_1
				GROUP BY drd_rfid_cd , drd_anten_no) AS sl_avg
			WHERE
				sl.drd_rfid_cd = sl_avg.drd_rfid_cd
					AND sl.drd_anten_no = sl_avg.drd_anten_no);`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, query)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func ClearDataOfShelfLocation(db *sql.DB) (bool, error) {
	query := `TRUNCATE TABLE shelf_location;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, query)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func ClearAndSetDataToShelfLocation(db *sql.DB) (bool, error) {
	if isSuccess, err := ClearDataOfShelfLocation(db); err != nil {
		return false, err
	} else if isSuccess {
		query := `INSERT INTO shelf_location
		SELECT 
			m1.drd_rfid_cd,
			m1.drd_anten_no,
			m1.drd_rssi,
			m1.drd_shelf_no,
			m1.m_10,
			m1.n_10
		FROM
			smart_shelf.drfid_raw_data m1
				INNER JOIN
			(SELECT 
				MAX(m_10) AS max_m_10, drd_rfid_cd, drd_shelf_no
			FROM
				smart_shelf.drfid_raw_data
			GROUP BY drd_shelf_no , drd_rfid_cd) m2 
			ON m1.drd_rfid_cd = m2.drd_rfid_cd
				AND m1.drd_shelf_no = m2.drd_shelf_no
				AND m1.m_10 = m2.max_m_10
		GROUP BY m1.drd_rfid_cd, m1.drd_shelf_no;`
		ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
		defer cancelfunc()
		tx, err := db.BeginTx(ctx, nil)
		if err != nil {
			log.Fatal(err)
		}

		_, err = tx.ExecContext(ctx, query)
		if err != nil {
			// Incase we find any error in the query execution, rollback the transaction
			log.Printf("Error %s when finding rows affected", err)
			tx.Rollback()
			return false, err
		}
		err = tx.Commit()
		if err != nil {
			log.Printf("Error %s when finding rows affected", err)
			return false, err
		}

		return true, nil
	} else {
		return false, err
	}
}

func SetSmartSelfLocation(db *sql.DB) (bool, error) {

	query := `INSERT INTO shelf_calc_location ( shelf_no, EPC, ROW, col ) (
		SELECT
			ma.shelf_no,
			EPC,
			ma.ROW,
			ma.scan_col_start + ma.direction * (slc.n_count - 1)
		FROM
			(
			SELECT
				sl.drd_anten_no AS n,
				ROW_NUMBER() OVER ( PARTITION BY sl.drd_anten_no ORDER BY sl.drd_rssi DESC ) AS n_count,
				sl.drd_rssi AS m_10,
				sl.drd_rfid_cd AS EPC,
				sl.drd_shelf_no as shelf_no

			FROM
				smart_shelf.drfid_raw_data AS sl,
				( SELECT drd_rfid_cd AS EPC, MAX( drd_rssi ) AS m_10 FROM drfid_raw_data GROUP BY 1 ) AS sl_max 
			WHERE
				sl.drd_rfid_cd = sl_max.EPC 
				AND sl.drd_rssi = sl_max.m_10 
			GROUP BY
				sl.drd_rfid_cd 
			) AS slc
		,MST_ANTENA ma WHERE slc.n = ma.antena_no AND ma.shelf_no = slc.shelf_no
		)`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, query)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func DeleteInvalidRecord(db *sql.DB) (bool, error) {

	query := `DELETE FROM shelf_calc_location WHERE col > 7 or col <= 0;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, query)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func SetSmartSelfLocation_OLD(db *sql.DB) (bool, error) {

	query := `INSERT INTO shelf_calc_location ( shelf_no, EPC, ROW, col ) (
		SELECT
			ma.shelf_no,
			EPC,
			ma.ROW,
			ma.col + ma.direction * (
			IF
				(
					(slc.n_count % ma.col_size) > 0,
					(slc.n_count DIV ma.col_size) + 1,
					(slc.n_count DIV ma.col_size) 
				)) AS col 
		FROM
			(
			SELECT
				sl.drd_anten_no AS n,
				ROW_NUMBER() OVER ( PARTITION BY sl.drd_anten_no ORDER BY sl.drd_rssi DESC ) AS n_count,
				sl.drd_rssi AS m_10,
				sl.drd_rfid_cd AS EPC 
			FROM
				smart_shelf.drfid_raw_data AS sl,
				( SELECT drd_rfid_cd AS EPC, MAX( drd_rssi ) AS m_10 FROM drfid_raw_data GROUP BY 1 ) AS sl_max 
			WHERE
				sl.drd_rfid_cd = sl_max.EPC 
				AND sl.drd_rssi = sl_max.m_10 
			GROUP BY
				sl.drd_rfid_cd 
			) AS slc
		LEFT JOIN MST_ANTENA ma ON ( slc.n = ma.antena_no ) 
		)`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, query)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func SetSmartSelfMoreInfo(db *sql.DB, EPC string, jancode string, product_name string, link_image string) (bool, error) {
	query := `UPDATE shelf_calc_location SET jancode = ?, product_name = ?, link_image = ?  WHERE EPC = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}
	_, err = tx.ExecContext(ctx, query, jancode, product_name, link_image, EPC)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func Test(db *sql.DB) (bool, error) {
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()

	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, "new_procedure")
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func GetSmartSelfLocation(db *sql.DB, shelf_name string) ([]Get_SmartSelf_Location, bool, error) {
	log.Printf("Getting setting of smart self")
	query := `select 
	EPC,
	jancode,
	product_name,
	link_image,
	row,
	col
	from shelf_calc_location
	WHERE shelf_no = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, false, err

	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, shelf_name)
	if err != nil {
		log.Printf("Error %s when query SQL statement", err)
		return nil, false, err
	}

	defer rows.Close()

	var data_list = []Get_SmartSelf_Location{}

	for rows.Next() {
		var data Get_SmartSelf_Location
		if err := rows.Scan(&data.EPC,
			&data.Jancode,
			&data.Product_name,
			&data.Link_image,
			&data.Row,
			&data.Col); err != nil {
			log.Println(err)
			return []Get_SmartSelf_Location{}, false, err
		}
		data_list = append(data_list, data)
	}

	if err := rows.Err(); err != nil {
		return data_list, false, err
	}
	return data_list, true, nil

}

func GetSmartSelfLocationByCol(db *sql.DB, shelf_name string, col int, row int) ([]Get_SmartSelf_Location, bool, error) {
	log.Printf("Getting setting of smart self")
	query := `select 
	EPC,
	jancode,
	product_name,
	link_image,
	row,
	col
	from shelf_calc_location
	WHERE shelf_no = ?
	AND col = ?
	AND row = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return nil, false, err

	}
	defer stmt.Close()
	rows, err := stmt.QueryContext(ctx, shelf_name, col, row)
	if err != nil {
		log.Printf("Error %s when query SQL statement", err)
		return nil, false, err
	}

	defer rows.Close()

	var data_list = []Get_SmartSelf_Location{}

	for rows.Next() {
		var data Get_SmartSelf_Location
		if err := rows.Scan(&data.EPC,
			&data.Jancode,
			&data.Product_name,
			&data.Link_image,
			&data.Row,
			&data.Col); err != nil {
			log.Println(err)
			return []Get_SmartSelf_Location{}, false, err
		}
		data_list = append(data_list, data)
	}

	if err := rows.Err(); err != nil {
		return data_list, false, err
	}
	return data_list, true, nil

}

func UpdateDirectionMSTAntena(db *sql.DB) (bool, error) {
	query := `UPDATE mst_antena set direction = case 
	when mod(antena_index,2) = 0 then -1
	when mod(antena_index,2) <> 0 then 1
	end;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}
	_, err = tx.ExecContext(ctx, query)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func ClearDirectionMSTAntena(db *sql.DB, shelf_no string) (bool, error) {
	query := `DELETE from smart_shelf.mst_antena WHERE shelf_no = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}
	_, err = tx.ExecContext(ctx, query, shelf_no)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func UpdatePositionMSTAntena(db *sql.DB, reqBody Set_SmartShelf_Position_mst_antena) (bool, error) {
	query := `UPDATE mst_antena SET direction = ?, row = ?, col = ?, col_size = ?,  scan_col_start = ?, scan_col_end = ? WHERE antena_no = ? AND shelf_no = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	//Convert to int and set value for direction + Set col
	direction := 1
	shelf_no_int, _ := strconv.Atoi(reqBody.Shelf_no)
	if shelf_no_int%2 == 0 {
		direction = -1
	} else {
		reqBody.Col = 0
		direction = 1
	}

	_, err = tx.ExecContext(ctx, query, direction, reqBody.Row, reqBody.Col, reqBody.ColSize, reqBody.Scan_col_start, reqBody.Scan_col_end, reqBody.Antena_no, reqBody.Shelf_no)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	UpdateDirectionMSTAntena(db)
	return true, err

}

func UpdatePositionMSTAntenaV1(db *sql.DB, reqBody Set_SmartShelf_Position_mst_antena) (bool, error) {
	query := `UPDATE mst_antena SET direction = ?, row = ?, col = ?, col_size = ?,  scan_col_start = ?, scan_col_end = ? WHERE antena_no = ? AND shelf_no = ?;`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	//Convert to int and set value for direction + Set col
	direction := 1
	shelf_no_int, _ := strconv.Atoi(reqBody.Shelf_no)
	if shelf_no_int%2 == 0 {
		direction = -1
	} else {
		reqBody.Col = 0
		direction = 1
	}

	_, err = tx.ExecContext(ctx, query, direction, reqBody.Row, reqBody.Col, reqBody.ColSize, reqBody.Scan_col_start, reqBody.Scan_col_end, reqBody.Antena_no, reqBody.Shelf_no)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	UpdateDirectionMSTAntena(db)
	return true, err

}

func InsertPositionMSTAntena_OLD(db *sql.DB) (bool, error) {

	query := `INSERT INTO mst_antena ( shelf_no, direction, scan_col_start, scan_col_end )`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	tx, err := db.BeginTx(ctx, nil)
	if err != nil {
		log.Fatal(err)
	}

	_, err = tx.ExecContext(ctx, query)
	if err != nil {
		// Incase we find any error in the query execution, rollback the transaction
		log.Printf("Error %s when finding rows affected", err)
		tx.Rollback()
		return false, err
	}
	err = tx.Commit()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}

	return true, err
}

func InsertPositionMSTAntena(db *sql.DB, reqBody Set_SmartShelf_Position_mst_antena) (bool, error) {
	log.Printf("Importing data to mst_antena table")
	query := `INSERT INTO mst_antena ( shelf_no, antena_no, antena_index, direction, row, col, col_size, scan_col_start, scan_col_end ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?);`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, err
	}
	defer stmt.Close()

	//Convert to int and set value for direction + Set col
	direction := 1
	shelf_no_int, _ := strconv.Atoi(reqBody.Shelf_no)
	if shelf_no_int%2 == 0 {
		direction = -1
	} else {
		reqBody.Col = 0
	}

	res, err := stmt.ExecContext(ctx, reqBody.Shelf_no, reqBody.Antena_no, reqBody.Antena_index, direction, reqBody.Row, reqBody.Col, reqBody.ColSize, reqBody.Scan_col_start, reqBody.Scan_col_end)
	if err != nil {
		log.Printf("Error %s when inserting row into log table", err)
		return false, err
	}
	rows, err := res.RowsAffected()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}
	log.Printf("%d rows created ", rows)
	UpdateDirectionMSTAntena(db)
	return true, nil
}

func InsertPositionMSTAntenaV1(db *sql.DB, reqBody Set_SmartShelf_Position_mst_antena) (bool, error) {
	log.Printf("Importing data to mst_antena table")
	query := `INSERT INTO mst_antena ( shelf_no, antena_no, direction, row, col, col_size, scan_col_start, scan_col_end ) VALUES (?, ?, ?, ?, ?, ?, ?, ?);`
	ctx, cancelfunc := context.WithTimeout(context.Background(), 5*time.Second)
	defer cancelfunc()
	stmt, err := db.PrepareContext(ctx, query)
	if err != nil {
		log.Printf("Error %s when preparing SQL statement", err)
		return false, err
	}
	defer stmt.Close()

	//Convert to int and set value for direction + Set col
	direction := 1
	shelf_no_int, _ := strconv.Atoi(reqBody.Shelf_no)
	if shelf_no_int%2 == 0 {
		direction = -1
	} else {
		reqBody.Col = 0
	}

	res, err := stmt.ExecContext(ctx, reqBody.Shelf_no, reqBody.Antena_no, direction, reqBody.Row, reqBody.Col, reqBody.ColSize, reqBody.Scan_col_start, reqBody.Scan_col_end)
	if err != nil {
		log.Printf("Error %s when inserting row into log table", err)
		return false, err
	}
	rows, err := res.RowsAffected()
	if err != nil {
		log.Printf("Error %s when finding rows affected", err)
		return false, err
	}
	log.Printf("%d rows created ", rows)
	UpdateDirectionMSTAntena(db)
	return true, nil
}
