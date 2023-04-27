package db_client

import (
	"context"
	"database/sql"
	"fmt"
	"log"
	"main/utils"
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
		log.Printf("Error %s when inserting row into log table", err)
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
			log.Println(err)
			log.Println(data.Dlm_id)
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
