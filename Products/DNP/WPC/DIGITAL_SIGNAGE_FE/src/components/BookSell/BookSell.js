import React from "react";
import Barcode from "react-barcode";
import styles from "./BookSell.module.css";

const BookSell = (props) => (
  <div className="col" style={{marginTop: `-0.2vh`}}>
    <div className={styles.col}>
      <div className={"flex flex-row justify-content-center " + props.classBind}>
        <div className={props.isHidden} style={{
          backgroundSize: `contain`,
          backgroundImage: `url(${props.top})`,
          backgroundRepeat: `no-repeat`,
          width: `5vh`,
        }}></div>
        <div className="textMedium">{props.rank}</div>
      </div>
      <div className={"flex flex-row justify-content-center "}>
        <img src={props.img} style={{height: `15vh`}} alt=""/>
      </div>
      <div className="textSmall" style={{height: '3.3vh'}}>{props.name}</div>
      <div className="barcode">
        {props.qrcode && <Barcode value={props.qrcode} format="EAN13" width={1} height={20} margin={5} displayValue={false} flat={true}/> }
      </div>
    </div>
  </div>
);

BookSell.propTypes = {};

BookSell.defaultProps = {
  props: {
    qrcode: "",
  }
};

export default BookSell;
