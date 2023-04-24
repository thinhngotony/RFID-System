CREATE TABLE `MST_ANTENA` (
  `antena_no` int(11) NOT NULL,
  `shelf_no` int(11) NOT NULL,
  `row` int(11) NOT NULL,
  `col` int(11) NOT NULL,
  `col_size` int(11) NOT NULL,
  `direction` int(11) NOT NULL,
  PRIMARY KEY (`antena_no`,`shelf_no`)
);