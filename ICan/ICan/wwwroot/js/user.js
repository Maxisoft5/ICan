function lastNameSorter(a, b) {
	let [isEmpty, result] = checkEmpty(a, b);
	if (isEmpty) {
		return result;
	}

	var dataA = $(a).data("lastname"); 
	var dataB = $(b).data("lastname"); ;
	if (dataA < dataB) return 1;
	if (dataA > dataB) return -1;
	return 0;
}
 