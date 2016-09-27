const mockupData = [
	{ "id": 1, "name": "BMW X5a", "brandID": 2, "modelID": 14, "modelName": "BMW X5", "groupID": 1, "groupName": "BMW Group 1", "year": "2014", "category": "SUV", "numOfSeat": 8, "transmission": "Automatic", "fuel": "Diesel"},
	{ "id": 2, "name": "BMW X6b", "brandID": 2, "modelID": 15, "modelName": "BMW X6", "groupID": 1, "groupName": "BMW Group 1", "year": "2015", "category": "SUV", "numOfSeat": 8, "transmission": "Automatic", "fuel": "Diesel"},
	{ "id": 3, "name": "BMW X3c", "brandID": 2, "modelID": 13, "modelName": "BMW X3", "groupID": 1, "groupName": "BMW Group 1", "year": "2016", "category": "SUV", "numOfSeat": 8, "transmission": "Automatic", "fuel": "Diesel"},
	{ "id": 4, "name": "Audi A7d", "brandID": 1, "modelID": 3, "modelName": "Audi A7", "groupID": 2, "groupName": "Audi Group 2", "year": "2014", "category": "Station Wagon", "numOfSeat": 8, "transmission": "Automatic", "fuel": "Diesel"},
	{ "id": 5, "name": "Audi A8e", "brandID": 1, "modelID": 4, "modelName": "Audi A8", "groupID": 2, "groupName": "Audi Group 2", "year": "2015", "category": "Station Wagon", "numOfSeat": 8, "transmission": "Automatic", "fuel": "Diesel"},
	{ "id": 6, "name": "Audi A8f", "brandID": 1, "modelID": 4, "modelName": "Audi A8", "groupID": 2, "groupName": "Audi Group 2", "year": "2016", "category": "Station Wagon", "numOfSeat": 8, "transmission": "Automatic", "fuel": "Diesel"},
	{ "id": 7, "name": "Ford Fiesta STg", "brandID": 3, "modelID": 18, "modelName": "Ford Fiesta Mk6", "groupID": 3, "groupName": "Ford Group 3", "year": "2014", "category": "Station Wagon", "numOfSeat": 8, "transmission": "Automatic", "fuel": "Diesel"}
];

// html star icons
const	fullStar = '<i class="fa fa-star"></i>',
		halfStar = '<i class="fa fa-star-half-o"></i>',
		emptyStar = '<i class="fa fa-star-o"></i>';

function renderStarRating(starRating){
	for(var html = '', star = starRating, i = 0; i < 5; i++) {
		if(star >= 1) {
			html += fullStar;
			star--;
		} else if (star > 0) {
			html += halfStar;
			star--;
		} else {
			html += emptyStar;
		}
	}
	return html += `&nbsp;<span class="badge">${starRating}</span>`
}

// Custom Filters Utils
// For simple text search
function createTextFilter(table, filterNode, colName){
	let col = table.column(`${colName}:name`);

	// filter button clicked
	filterNode.find('.dropdown-menu button').click((event) => {
		col.search(filterNode.find('.dropdown-menu input').val()).draw();
		filterNode.find('.filter-toggle').addClass('btn-success');
	});

	// enter pressed
	$(".dropdown-menu input").keyup((event) => {
		if (event.keyCode == 13) {
			col.search(event.target.value).draw();
			filterNode.find('.filter-toggle').addClass('btn-success');
		}
	});

	// clear filter event
	filterNode.find('.filter-remove').click((event) => {
		col.search('').draw();
		filterNode.find('.filter-toggle').removeClass('btn-success');
		filterNode.removeClass('open');
	});
}

// For tree checkbox-like
function createTreeFilter(table, filterNode, filterCol, tree){
	let selectedItem = [];

	$.fn.dataTable.ext.search.push((settings, data) => {
		if(!(selectedItem.length === 0)){
			return selectedItem.find((item) => {
				if(item.nodeLvl == 1){
					console.log(data[filterCol[0]], item.nodeI);
					return data[filterCol[0]] == item.nodeId;
				} else if(item.nodeLvl == 2){
					console.log(data[filterCol[1]], item.nodeI);
					return data[filterCol[1]] == item.nodeId;
				}
				return false;
			});
		}
		return true;
	});

	// filter button clicked
	filterNode.find('.dropdown-menu button').click((event) => {
		selectedItem = tree.get_top_selected(true).map((item) => {
			console.log(item.data);
			return item.data;
		});
		table.draw();
		filterNode.find('.filter-toggle').addClass('btn-success');
	});

	// clear filter event
	filterNode.find('.filter-remove').click((event) => {
		selectedItem = [];
		table.draw();
		filterNode.find('.filter-toggle').removeClass('btn-success');
		filterNode.removeClass('open');
	});
}

// For checkbox-like
function createCheckboxFilter(table, filterNode, filterCol){
	let selectedItem;

	$.fn.dataTable.ext.search.push((settings, data) => {
		if(selectedItem)
			return selectedItem.includes(data[filterCol]);

		return true;
	});

	// filter button clicked
	filterNode.find('.dropdown-menu button').click((event) => {
		selectedItem = filterNode.find('input:checked').toArray().map((checkbox) => {
			return checkbox.value;
		});
		table.draw();
		filterNode.find('.filter-toggle').addClass('btn-success');
	});

	// clear filter event
	filterNode.find('.filter-remove').click((event) => {
		selectedItem = null;
		table.draw();
		filterNode.find('.filter-toggle').removeClass('btn-success');
		filterNode.removeClass('open');
	});
}

// For range-like with int data
function createIntRangeFilter(table, filterNode, filterCol){
	let min, max;

	$.fn.dataTable.ext.search.push((settings, data) => {
		return ( min ? ( data[filterCol] >= min ) : true )
			&& ( max ? ( data[filterCol] <= max ) : true );
	});

	// filter button clicked
	filterNode.find('.dropdown-menu button').click((event) => {
		min = Number.parseInt(filterNode.find('.from-input').val());
		max = Number.parseInt(filterNode.find('.to-input').val());
		table.draw();
		filterNode.find('.filter-toggle').addClass('btn-success');
	});

	// clear filter event
	filterNode.find('.filter-remove').click((event) => {
		min = max = null;
		table.draw();
		filterNode.find('.filter-toggle').removeClass('btn-success');
		filterNode.removeClass('open');
	});
}

$(document).ready(function(){
	// Intialize location selector
	$('#locationID').chosen({
		width: "100%",
		no_results_text: "No result!"
	});

	// Render star-rating
	let starRatingDiv = $('#starRating'),
		star = starRatingDiv.data('star')

	starRatingDiv.html(renderStarRating(star));

	// Intinialize open/close time-picker
	const timepickerConfig = {
		showMeridian: false,
		defaultTime: false
	};
	$('#openTimeMon').timepicker(timepickerConfig);
	$('#closeTimeMon').timepicker(timepickerConfig);
	$('#openTimeTue').timepicker(timepickerConfig);
	$('#closeTimeTue').timepicker(timepickerConfig);
	$('#openTimeWed').timepicker(timepickerConfig);
	$('#closeTimeWed').timepicker(timepickerConfig);
	$('#openTimeThur').timepicker(timepickerConfig);
	$('#closeTimeThur').timepicker(timepickerConfig);
	$('#openTimeFri').timepicker(timepickerConfig);
	$('#closeTimeFri').timepicker(timepickerConfig);
	$('#openTimeSat').timepicker(timepickerConfig);
	$('#closeTimeSat').timepicker(timepickerConfig);
	$('#openTimeSun').timepicker(timepickerConfig);
	$('#closeTimeSun').timepicker(timepickerConfig);

	// ============================================
	// Vehicle table

	// render model-tree selector
	let modelTree = $.jstree.create('#modelTree', {
		core: {
			dblclick_toggle: false,
			themes: {
				icons: false,
				variant: "small"
			}
		},
		plugins: ["checkbox", "wholerow"]
	});

	// set toogling dropdown event for filter dropdown buttons
	$('#multiFilter .filter-toggle').on('click', function (event) {
		let dropdownContainer = $(this).parent();

		if(dropdownContainer.hasClass('open')){
			$('#multiFilter .filter-toggle').parent().removeClass('open');
		} else {
			$('#multiFilter .filter-toggle').parent().removeClass('open');
			dropdownContainer.addClass('open');
		}
	});

	// Load vehicles belonging to this garage
	let table = $('#vehicles').DataTable({
		data: mockupData,
		dom: 'ltipr',
		lengthMenu: [ 10, 25, 50 ],
		processing: true,
		select: {
			selector: 'td:not(:last-child)',
			style: 'multi+shift'
		},
		columnDefs: [
			{
				// Render action button
				targets: 12,
				render: (data, type, row) => {
					return `<div class="btn-group" >
						<button data-toggle="dropdown" class="btn btn-info btn-block dropdown-toggle" aria-expanded="false">
							<i class="fa fa-gear"></i> Actions <i class="caret"></i>
						</button>
						<ul class="dropdown-menu">
							<li><a href="#" data-toggle="modal" data-target="#mdModal" data-action="changeGarage" data-id="${row.id}" >Change Garage</a></li>
							<li><a href="#" data-toggle="modal" data-target="#mdModal" data-action="changeGroup" data-id="${row.id}" >Change Group</a></li>
							<li><a href="#" data-toggle="modal" data-target="#smModal" data-action="delete" data-id="${row.id}" data-name="${row.name}" >Delete</a></li>
							<li><a href="#" data-toggle="modal" data-target="#bgModal" data-action="duplicate" data-id="${row.id}" >Duplicate</a></li>
							<li><a href="./../car/car.html">Edit</a></li>
						</ul>
					</div>`;
				}
			}
		],
		columns: [
			{ name: 'ID', data: 'id', type: 'num', visible: false },
			{ name: 'BrandID', data: 'brandID', type: 'num', visible: false },
			{ name: 'ModelID', data: 'modelID', type: 'num', visible: false },
			{ name: 'GroupID', data: 'groupID', type: 'num', visible: false },
			{ name: 'Name', title: 'Name', data: 'name', width: '20%' },
			{ name: 'Model', title: 'Model', data: 'modelName', width: '15%' },
			{ name: 'Category', title: 'Category', data: 'category', width: '10%' },
			{ name: 'Year', title: 'Year', data: 'year', width: '5%' },
			{ name: 'Seat', title: 'Seat', data: 'numOfSeat', width: '5%' },
			{ name: 'Transmission', title: 'Transmission', data: 'transmission', width: '10%' },
			{ name: 'Fuel', title: 'Fuel', data: 'fuel', width: '10%' },
			{ name: 'Group', title: 'Group', data: 'groupName', width: '15%' },
			{
				name: 'Action', 
				title: 'Action',
				width: '10%',
				orderable: false,
				searchable: false
			}
		]
	});

	// Bind the filters with table

	// Vehicle's name filter
	createTextFilter(table, $('#vehicleNameFilter'), 'Name');
	// Model filter
	createTreeFilter(table, $('#modelFilter'), [1, 2], modelTree);
	// Category filter
	createCheckboxFilter(table, $('#categoryFilter'), 6);
	// Year filter
	createIntRangeFilter(table, $('#yearFilter'), 7);
	// Seat filter
	createIntRangeFilter(table, $('#seatFilter'), 8);
	// Transmission filter
	createCheckboxFilter(table, $('#transmissionFilter'), 9);
	// Fuel filter
	createCheckboxFilter(table, $('#fuelFilter'), 10);
	// Vehicle Group filter
	createCheckboxFilter(table, $('#groupFilter'), 3);
});