import React from "react";
import styles from "./BookOnShelf.module.css";

const BookOnShelf = (props) => (
  <div className="col">
    <div className="row" >
      <div className="col" style={{paddingTop: `5.3vh`}}>
        <div className="d-flex justify-content-center align-items-center" style={{fontSize: `2vh`, margin: `0px`}}>
            <div className={styles.circle} style={{fontSize: `1.5vh`}}>{props.rank}</div>
        </div>
        <div className="d-flex justify-content-center align-items-center" style={{fontSize: `1.5vh`, margin: `0px`}}>
            <div>{props.view_time_in_seconds}</div>
        </div>
        <div className="d-flex justify-content-center align-items-center" style={{fontSize: `1.5vh`, margin: `0px`}}>
            <div>{props.view_cnt}</div>
        </div>
        <div className="d-flex justify-content-center" style={{position: 'relative'}}>
          <img className={styles.book} src={props.img || "../parts/noimage.png"}/>
          <div className={styles.noImageText}>{props.img === '' ? props.name : ''}</div>
        </div>
      </div>
    </div>
  </div>
);

BookOnShelf.defaultProps = {};

export default BookOnShelf;
