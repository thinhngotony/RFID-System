import React from "react";
import styles from "./BookOnTable.module.css";

const BookOnTable = (props) => (
  <div className="col">
    <div className="row" >
      <div className="col">
        <div className="d-flex justify-content-center align-items-center" style={{fontSize: `1.5vh`}}>
            <div className={styles.circle} style={{fontSize: `1.3vh`}}>{props.rank}</div>
            <div>{props.view_time_in_seconds}/</div>
            <div>{props.view_cnt}</div>
        </div>
        <div className="d-flex justify-content-center" style={{position: 'relative', transform: 'rotate(-90deg)', fontSize: '1vh', marginTop: '-1vh'}}>
          <img className={styles.book} src={props.img || "../parts/noimage.png"}/>
          <div className={styles.noImageText}>{props.img === '' ? props.name : ''}</div>
        </div>
      </div>
    </div>
  </div>
);

BookOnTable.defaultProps = {};

export default BookOnTable;
