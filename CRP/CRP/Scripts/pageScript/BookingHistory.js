﻿const viDatatables = {
    lengthMenu: "Hiển thị _MENU_ dòng",
    search: "Tìm kiếm",
    paginate: {
        first: "Trang đầu",
        previous: "Trang trước",
        next: "Trang sau",
        last: "Trang cuối",
    },
    zeroRecords: "Không tìm thấy dữ liệu",
    info: "Đang hiển thị trang _PAGE_ trên tổng số _PAGES_ trang",
    infoEmpty: "không có dữ liệu",
    infoFiltered: "(được lọc ra từ _MAX_ dòng)"
}
$(document).ready(() => {
    // set toogling dropdown event for filter dropdown buttons
    $('#multiFilter .filter-toggle').on('click', function (event) {
        let dropdownContainer = $(this).parent();

        if (dropdownContainer.hasClass('open')) {
            $('#multiFilter .filter-toggle').parent().removeClass('open');
        } else {
            $('#multiFilter .filter-toggle').parent().removeClass('open');
            dropdownContainer.addClass('open');
        }
    });

    var searchCondition = {

    };
    // Render table
    let table = $('#garages').DataTable({
        dom: "ltipr",
        //data: mockupData,
        language: viDatatables,
        ajax: {
            url: "/api/BookingHistorys",
            type: "GET",
            //data: searchCondition
        },
        columnDefs: [
             {
                 // Render status label
                 targets: 2,
                 render: (data, type) => {
                     return Date(data).toString();
                 }

             },

            {
                // Render status label
                targets: 3,
                render: (data, type) => {
                    return Date(data).toString();
                }
                
            },

			{
			    // Render status label
			    targets: 4,
			    render: (data, type) => {
			        if (type === 'display') {
			            return `<div class="status-label" >
							<p class ="label label-${data ? 'danger' : 'primary'}">${data ? 'Đã hủy hoặc hoàn thành': 'During'}</p>
						</div>`;
			        }
			        return data;
			    }
			},
            {
                // Render stars
                targets: 5,
                render: (data, type) => {
                    if (type === 'display') {
                        return renderStarRating(data);
                    }
                    return data;
                }
            },
			{
			    // Render action button
			    targets: 12,
			    render: (data, type, row) => {
			        return `<div class="btn-group" >
						<button data-toggle="dropdown" class="btn btn-info dropdown-toggle" aria-expanded="false">
							<i class="fa fa-gear"></i> Actions <i class="caret"></i>
						</button>
						<ul class ="dropdown-menu">
                        	<li><a href="#" data-toggle="modal" data-target="#mdModal" data-action="detail" data-id="${row[0]}"
                            data-name="${row[1]}" data-starttime="${row[2]}" data-endtime="${row[3]}" data-rentalprice="${row[6]}"
                            data-bookingfee="${row[7]}" data-garage="${row[8]}" data-garageadd="${row[9]}" data-color="${row[10]}"
                            data-model="${row[11]}" data-star="${row[5]}"
                            >Chi tiết</a></li>
							${row[4] === true ?
								`<li><a href="#" data-toggle="modal" data-target="#mdModal" data-action="comment" data-id="${row[0]}">Nhận Xét</a></li>`
                                :
								`<li><a href="#" data-toggle="modal" data-target="#mdModal" data-action="cancle" data-id="${row[0]}">Hủy</a></li>`}
						</ul>
					</div>`;
			    }
			}
        ],
        columns: [
			{ name: 'ID', visible: false },
			{ name: 'VehicleName', title: 'VehicleName', width: '15%' },
			{ name: 'Start Time', title: 'Start Time', width: '15%' },
			{ name: 'End Time', title: 'End Time', width: '15%' },
            { name: 'Status', title: 'Status',  width: '30%' },
			{ name: 'Star', title: 'Star', width: '10%' },
            { name: 'RentalPrice', visible: false },
            { name: 'BookingFee', visible: false },
            { name: 'GarageName', visible: false },
            { name: 'GarageAddress', visible: false },
            { name: 'Color', visible: false },
            { name: 'Model', visible: false },
			{
			    title: 'Action',
			    width: '10%',
			    orderable: false,
			    searchable: false
			}
        ]
    });

    // garage's name text filter
    createTextFilter(table, $('#VehicleName'), 'VehicleName');
    // garage's name text filter
    createTextFilter(table, $('#StartTime'), 'Start Time');
    // garage's address text filter
    createTextFilter(table, $('#EndTime'), 'End Time');
    // garage's name text filter
    createTextFilter(table, $('#Status'), 'Status');
    // garage's name text filter
    createTextFilter(table, $('#Star'), 'Star');

    // Render confirmation modal for actions
    $('#mdModal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget),
			action = button.data('action')
            id = button.data('id')
            VehicleName = button.data('name')
            starTime = button.data('starttime')
            star = button.data('star')
            endTime = button.data('endtime')
            RentalPrice = button.data('rentalprice')
            BookingFee = button.data('bookingfee')
            GarageName = button.data('garage')
            GarageAddress = button.data('garageadd')
            Color = button.data('color')
            Model = button.data('model')
        switch (action) {
            case 'detail': {
                $(this).find('.modal-content').html(`<div class="row" style="text-align:center; margin-top:10px">
											<h3 style="font-size:200%;">Booking of</h3>
											  <div class="col-sm-6 b-r" style="float: left">
												<div class ="form-group"><label>Rental Price</label><p>${RentalPrice}</p></div>
                                                <div class ="form-group"><label>Booking Fee</label><p>${BookingFee}</p></div>
                                                <div class ="form-group"><label>Star</label><p>${renderStarRating(star)}</p></div>
                                                <div class ="form-group"><label>Start time</label><p>${Date(starTime).toString()}</p></div>
									            <div class ="form-group"><label>End time</label><p>${Date(endTime).toString()}</p></div>
                                              </div>
								              <div class ="col-sm-6" style="float: right">
                                               <div class ="form-group"><label>Vehicle Name</label><p>${VehicleName}</p></div>
                                               <div class ="form-group"><label>Model</label><p>${Model}</p></div>
                                               <div class ="form-group"><label>Color</label><p>${Color}</p></div>
									           <div class ="form-group"><label>Garage Name</label><p>${GarageName}</p></div>
									           <div class ="form-group"><label>Garage Address</label><p>${GarageAddress}</p></div>
                                              </div>
                                               <button type="button" class ="btn btn-default" data-dismiss="modal">Close</button>
									            <label></label>	<br/>`);
            }
            break;
            case 'comment': {
                $(this).find('.modal-content').html(`<div class="row" style="text-align:center; margin-top:30px">
											<h3 style="font-size:200%;">Comment And Rate ${VehicleName}</h3>
                                  
											  <div class="col-sm-12" style="text-align: center">
												<label>Comment</label>
                                                <input type="text" class="form-control" id="comment">
                                                <input type="hidden" class ="form-control" value="${id}" id="id" />
                                                <label>Rate</label>
                                                 <input type="text" class ="form-control" id="star">
                                              </div>
                                               <label></label>	<br/>
                                              <button type="button" class ="btn btn-default" data-dismiss="modal">Close</button>
                                              <button type="button" class ="btn btn-success btn-yeah">Submit</button>
									            <label></label>	<br/>`);
               
                }
            break;
            case 'cancle': {
                $(this).find('.modal-content').html(`<div class="modal-header">
			<button type="button" class="close" data-dismiss="modal" aria-label="Close">
				<span aria-hidden="true">&times;</span>
			</button>
			<h2 class="modal-title">
				${action === 'delete' ? 'Deletion' : (action === 'deactivate' ? 'Deactivation' : 'Activation')} Confirmation
			</h2>
		    </div>
		    <div class="modal-body">
			You are about to <b>${action}</b> this booking?</b>. Are you sure?
		    </div>
		    <div class="modal-footer">
			<button type="button" class="btn btn-default" data-dismiss="modal">No</button>
			<button type="button" class="btn btn-danger btn-yes">Yes</button>
		    </div>`);
            }
            break;   
            }
            $(document).on('click', '.btn-yes', function (event) {
            $.ajax({
                url: `/api/booking/status/${id}`,
                type: "DELETE",
                success: function (data) {
                    alert("ok");
                    location.href = "/management/bookingHistory";
                },
                eror: function (data) {
                    alert("fail");
                    location.href = "/management/bookingHistory";
                }
                });
            });
            
            $(document).on('click', '.btn-yeah', function (event) {
                $("#error").html("");
                var id = $('#id').val();
                var comment = $('#comment').val();
                var star = $('#star').val();
                $.ajax({
                    url: '/api/CommentBooking',
                    data: {
                        id: id,
                        comment: comment,
                        star: star,
                    },
                    error: function () {
                        alert("Problem")
                        location.href = "/management/bookingHistory";
                    },
                    success: function (data) {
                        alert("Comment successfully");
                        location.href = "/management/bookingHistory";
                    },
                    type: 'POST'
                });
            });
            
    });
});