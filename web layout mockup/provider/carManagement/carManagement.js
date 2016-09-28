/** carManagement.js ver1.0 */
// html star icons
const	fullStar = '<i class="fa fa-star"></i>',
        halfStar = '<i class="fa fa-star-half-o"></i>',
        emptyStar = '<i class="fa fa-star-o"></i>';
var data = [
        [
            "1",
            "59H-123.22",
            "ABC",
            "Grp A",
            "Honda",
            "SUV",
            "2.5L",
            "Petrol",
            "Automatic",
            "5000 HP",
            "5",
            "3.5",
        ],
        [
            "2",
            "give",
            "me",
            "more",
            "times",
            "I",
            "will",
            "improve",
            "that",
            "is",
            "5",
            "g",
            ]
    ];
$(document).ready(function() {

    // set toogling dropdown event for filter dropdown buttons
    $('#multiFilter .dropdown-toggle').on('click', function (event) {
        let dropdownContainer = $(this).parent();

        if(dropdownContainer.hasClass('open')){
            $('#multiFilter .dropdown-toggle').parent().removeClass('open');
        } else {
            $('#multiFilter .dropdown-toggle').parent().removeClass('open');
            dropdownContainer.addClass('open');
        }
    });

//            $('#table-magmt tfoot th').each( function (i) {
//                var title = $('#table-magmt tfoot th').eq( $(this).index() ).text();
//                $(this).html( '<input type="text" placeholder="Search '+title+'" data-index="'+i+'" />' );
//            } );

//            $('#table-magmt tfoot th').each( function () {
//                var title = $(this).text();
//                $(this).html( '<input type="text" placeholder="Search '+title+'" />' );
//            } );

    var table = $('#table-magmt').dataTable({
        "dom" : 'lrtip',
        //"sDom": '<"top"if>rt<"bottom"lp><"clear">',
        "data" : data,
        //"bSort": false,
        "retrieve": true,
        //"bServerSide": true,
        "scrollCollapse": true,
        "processing": true,
        "select": {
            style: 'multi'
        },
        //"iDisplayLength": 10,
        //"aLengthMenu": [10, 25, 50],
        "columns" : [
            { visible: false },
            { title: "License"},
            { title: "Garage"},
            { title: "Group"},
            { title: "Brand"},
            { title: "Type"},
            { title: "Engine", visible: false},
            { title: "Fuel"},
            { title: "Transmission"},
            { title: "Power", visible: false},
            { title: "Seat"},
            { title: "Rate"},
            { title: "Action"},
        ],
        "columnDefs": [
//                    {
//                        "aTargets": [0, 1, 2, 3, 4, 5, 6, 7, 8, 9],
//                        //"bSortable": false
//                    },
            {
                "targets": [11],
                "render": function(data, type, o) {
                    if(type === 'display'){
                        for(var html = '', star = data, i = 0; i < 5; i++) {
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
                        return html += `&nbsp;&nbsp;<span class="badge">${data}</span>`;
                    }
                    return data;
                }
            },
            {
                "targets": [12],
                "render": function (data, type, o) {
                    var action = `<div class="btn-group" >
                <button data-toggle="dropdown" class="btn btn-primary btn-sm dropdown-toggle" aria-expanded="false">
                    <i class="fa fa-gear"></i> Actions <i class="caret"></i>
                </button>
                <ul class="dropdown-menu">
                    <li><a href="#" data-toggle="modal" class="font-bold" data-target="#car-detail">Duplicate</a></li>
                    <li><a href="./../car/car.html" class="font-bold">Edit</a></li>

                    <li><a href="#" data-toggle="modal" class="font-bold" data-target="#confirmModal" data-action="delete" data-name="${o[1]}" data-id="${o[0]}">Delete</a></li>
                </ul>
            </div>`;
                    var edit='<a class="btn btn-edit btn-primary btn-sm">Edit</a>'
                    var del='<a class="btn btn-edit btn-danger btn-sm">Delete</a>'
                    return action;
                },
                "sortable": false,
            },
        ],
    });

//            table.columns().every(function() {
//                var that = this;
//                $('input', this.footer()).on( 'keyup change', function() {
//                if(that.search() !== this.value) {
//                    that.search( this.value ).draw();
//                    }
//                });
//            });

    $('#car-color').css("color", $('#color-name').text());

    Dropzone.options.myAwesomeDropzone = {

        autoProcessQueue: false,
        uploadMultiple: true,
        acceptedFiles: "image/jpeg,image/png,image/gif",
        parallelUploads: 20,
        maxFiles: 20,
        maxFilesize: 1,
        dictDefaultMessage: "Drop files here to upload (or click)",
        dictInvalidFileType: "Accept image only",
        addRemoveLinks: "dictRemoveFile",

        // Dropzone settings
        init: function() {
            var myDropzone = this;

            this.element.querySelector('input[name="submit-img"]').addEventListener("click", function(e) {
                e.preventDefault();
                e.stopPropagation();
                myDropzone.processQueue();
            });
            this.on("sendingmultiple", function() {
                alert("sending");
            });
            this.on("successmultiple", function(files, response) {
                alert("success");
            });
            this.on("errormultiple", function(files, response) {
                alert("fail");
            });
        }

    }

    //Render change modal
    $('#changeModal').on('show.bs.modal', function(e) {
        let button = $(e.relatedTarget),
            name = button.data('name');

        $(this).find('.modal-content').html(
            `<div class="modal-header">
                <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                    <span aria-hidden="true">&times;</span>
                </button>
                <h2 class="modal-title">
                    Change ${name}
                </h2>
            </div>
            <div class="modal-body">
                <div class="row">
                    <div class="col-md-4 text-right">
                        <label>Select a ${name}</label>
                    </div>
                    <div class="col-md-8">
                        <select class="form-control">
                            <option value="1">a</option>
                            <option value="2">b</option>
                            <option value="3">c</option>
                            <option value="4">d</option>
                            <option value="5">e</option>
                        </select>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-success">OK</button>
                <button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button>
            </div>`
        );
    });

    // Render confirmation modal for actions
    $('#confirmModal').on('show.bs.modal', function (event) {
        let button = $(event.relatedTarget),
            action = button.data('action')
            id = button.data('id'),
            name = button.data('name');

        $(this).find('.modal-content').html(`<div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
            <h2 class="modal-title">
                Deletion Confirmation
            </h2>
        </div>
        <div class="modal-body">
            You are about to <b>delete</b> car <b>${name}</b>. Are you sure?
        </div>
        <div class="modal-footer">
            <button type="button" class="btn btn-default" data-dismiss="modal">No</button>
            <button type="button" class="btn btn-danger">Yes</button>
        </div>`);
    });
});