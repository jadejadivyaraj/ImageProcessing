

//Global Variables
var StadiumData = undefined; 
var countofSelectedImages = 0;
var current_progress = 0;
$(document).ready(function () {


    //Sucess alert hiding.
    //$("#success-success").hide();

    //Global Progressbar hiding.
    //$("#dynamic").hide();

    // File Upload data with settings
    $("#success-success").slideUp();
    var settings = {
        url: "https://localhost:44397/uploader/SubmitFormUpload",
        method: "POST",
        allowedTypes: "jpg,png,gif,jpeg,JPG,JPEG,PNG,GIF",
        fileName: "myfile",
        cache: false,
        autoSubmit: false,
        showDone: true,
        multiple: true,
        dynamicFormData: function () {
            var data = {
                "stadiumName": $("#stadiumName").val(),
                "eventDate": $("#eventDate").val()
            }
            return data;
        },
        onSelect: function (files) {

            console.log(files[0].name + " -" + files[0].size + " Count :" + files.length);
            countofSelectedImages = files.length;
            return true; 
        },
        onSubmit: function (files) {
            $("#eventsmessage").html($("#eventsmessage").html() + "<br/>Submitting:" + JSON.stringify(files));
            console.log(files.length);
        },
        onSuccess: function (files, data, xhr) {
            console.log(files);
            console.log(data);
            current_progress = current_progress+1;
            var percentage = Math.floor(current_progress / countofSelectedImages * 100);
            console.log(percentage);
            $("#dynamic").css("visibility","visible");
            $("#progress-bargloabal")
                .css("width", percentage + "%")
                .attr("aria-valuenow", percentage )
                .text(percentage + "% Complete");
        },
        afterUploadAll: function (obj) {
            $("#success-success").css("visibility","visible");
            $("#success-success").fadeTo(2000, 1000).slideDown(2000, function () {
                $("#success-success").slideUp(1000);
            });

            $("#eventDate").val("");
            $("#stadiumName").val("");

            extraObj.reset();
            countofSelectedImages = 0;
            current_progress = 0;
            $("#dynamic").css("visibility", "hidden");
        },
        onError: function (files, status, errMsg) {
            $("#status").html("<font color='red'>Upload is Failed</font>");
        }
    }

    //FileUpload object
    var extraObj = $("#mulitplefileuploader").uploadFile(settings);

    //FileUploadPage -Submit Button
    $("#btnSubmit").click(function () {

        document.body.scrollTop = 0;
        document.documentElement.scrollTop = 0;
        if ($("#stadiumName").val() !== "") {
            if ($("#eventDate").val() !== "")
            {
                if (countofSelectedImages > 0) {
                    
                    $('#spnDocMsg_fileupload').text("").hide();
                    $("#dynamic").show();
                    extraObj.startUpload();
                }
                else
                {
                    $('#spnDocMsg_fileupload').text("No Images found, Please select images to upload server").show();
                }
                
            }
        }
        else
        {
            $('#spnDocMsg_fileupload').text("Sorry!! Please fill the all required details").show();
        }

        
        
    });

    //SearchPage - Date Dropdown
    $("#menu1").prop('disabled', true);

    //SearchPage -FileUpload
    $("#FileUpload1").change(function () {
        console.log($(this).get(0).files[0].size);
        // Get uploaded file extension
        var extension = $(this).val().split('.').pop().toLowerCase();
        // Create array with the files extensions that we wish to upload
        var validFileExtensions = ['jpeg', 'jpg', 'png', 'gif', 'bmp'];
        //Check file extension in the array.if -1 that means the file extension is not in the list. 
        if ($.inArray(extension, validFileExtensions) == -1) {
            $('#spnDocMsg').text("Sorry!! Upload only jpg, jpeg, png, gif, bmp file").show();
            // Clear fileuload control selected file
            $(this).replaceWith($(this).val('').clone(true));
            //Disable Submit Button
            $('#submitbutton_search').prop('disabled', true);
        }
        else {
            // Check and restrict the file size to 10MB.
            if ($(this).get(0).files[0].size > (10048576)) {
                $('#spnDocMsg').text("Sorry!! Max allowed file size is 10 MB").show();
                // Clear fileuload control selected file
                $(this).replaceWith($(this).val('').clone(true));
                //Disable Submit Button
                $('#submitbutton_search').prop('disabled', true);
            }
            else {
                //Clear and Hide message span
                $('#spnDocMsg').text('').hide();
                //Enable Submit Button
                $('#submitbutton_search').prop('disabled', false);
            }
        }
    });

    //---- calling API and Filling AutoComplete Data
    /*An array containing all the stadium names:*/

    //calling API to fill the Auto Complete with Stadium Names
    GetStadiumInfo();

    /*initiate the autocomplete function on the "myInput" element, and pass along the StadiumData array as possible autocomplete values:*/
    autocomplete(document.getElementById("myInput"), StadiumData);

    //$("#myInput").keydown(function () {
        
    //    if ($("#myInput").val() !== "")
    //    {
    //        if (checkStadiumNameExistOrNot($("#myInput").val())) {
    //            var stName = $("#myInput").val();
    //            var url = "https://localhost:44397/uploader/GetDatesOfStadium?stdName=" + stName;
    //            $.ajax({
    //                method: "GET",
    //                dataType: "json",
    //                url: url,
    //                async: false,
    //                context: document.body,
    //                success: function (data) {
    //                    var dates = data;
    //                    if (dates.length > 0) {
    //                        //clear the all the option values
    //                        //then add new value
    //                        $('#datetimedata')
    //                            .empty();

    //                        for (var i = 0; i < dates.length; i++) {
    //                            $('#datetimedata')
    //                                .append(' <li style="cursor:pointer;">' + dates[i] + '</li>');
    //                        }
    //                        $(".dropdown .dropdown-menu li")[0].click();
    //                        $('#spnDocMsg').hide();
    //                        $("#menu1").prop('disabled', false);
    //                        $("#submitbutton").prop('disabled', false);
    //                    }
    //                }
    //            });
    //        }
    //        else
    //        {
    //            $('#spnDocMsg').text("Sorry!! Stadium name is not valid,kindly check stadium name").show();
    //            if ($(".dropdown .dropdown-menu li").length > 0) {
    //                $(".dropdown .dropdown-menu li")[0].innerText = "Event Date";
    //                $(".dropdown .dropdown-menu li")[0].click();
    //            }
    //            $("#menu1").prop('disabled', true); 
    //            $("#submitbutton").prop('disabled', true);
    //        }
    //    }
    //    else
    //    {
           
            
    //    }

    //});

    $("#myInput").focusout(
        function ()
        {
            if ($("#myInput").val() === "") {
                $('#spnDocMsg').text("Sorry!! Stadium name should not be null").show();
                if ($(".dropdown .dropdown-menu li").length > 0) {
                    $(".dropdown .dropdown-menu li")[0].innerText = "Event Date";
                    $(".dropdown .dropdown-menu li")[0].click();
                }
                $("#menu1").prop('disabled', true);
                $("#submitbutton").prop('disabled', true);
            }
            else
            {
                if (checkStadiumNameExistOrNot($("#myInput").val()))
                {
                    doGetDates();
                    $('#spnDocMsg').hide();
                    $("#submitbutton").prop('disabled', false);
                }
                else
                {
                    if ($("#myInputautocomplete-list div").length > 0)
                    {
                        $("#myInput").val($("#myInputautocomplete-list")[0].innerText);
                    }
                    else
                    {
                        $('#spnDocMsg').text("Sorry!! Stadium name is not valid,kindly check stadium name").show();
                        if ($(".dropdown .dropdown-menu li").length > 0) {
                            $(".dropdown .dropdown-menu li")[0].innerText = "Event Date";
                            $(".dropdown .dropdown-menu li")[0].click();
                        }
                        $("#menu1").prop('disabled', true);
                        $("#myInput").val("");
                    }
                    //alert("not valid");
                }
            }
        }
    );

    $('.dropdown-menu').on('click', 'li', function () {
        var text = $(this).html();
        alert(text);
        $("#input_datetime").val(text);
        var htmlText = text + ' <span class="caret"></span>';
        $(this).closest('.dropdown').find('.dropdown-toggle').html(htmlText);
    });
});
function autocomplete(inp, arr) {
    /*the autocomplete function takes two arguments,
    the text field element and an array of possible autocompleted values:*/
    var currentFocus;
    /*execute a function when someone writes in the text field:*/
    inp.addEventListener("input", function (e) {
        var a, b, i, val = this.value;
        /*close any already open lists of autocompleted values*/
        closeAllLists();
        if (!val) { return false; }
        currentFocus = -1;
        /*create a DIV element that will contain the items (values):*/
        a = document.createElement("DIV");
        a.setAttribute("id", this.id + "autocomplete-list");
        a.setAttribute("class", "autocomplete-items");
        /*append the DIV element as a child of the autocomplete container:*/
        this.parentNode.appendChild(a);
        /*for each item in the array...*/
        for (i = 0; i < arr.length; i++) {
            /*check if the item starts with the same letters as the text field value:*/
            if (arr[i].substr(0, val.length).toUpperCase() == val.toUpperCase()) {
                /*create a DIV element for each matching element:*/
                b = document.createElement("DIV");
                /*make the matching letters bold:*/
                b.innerHTML = "<strong>" + arr[i].substr(0, val.length) + "</strong>";
                b.innerHTML += arr[i].substr(val.length);
                /*insert a input field that will hold the current array item's value:*/
                b.innerHTML += "<input type='hidden' value='" + arr[i] + "'>";
                /*execute a function when someone clicks on the item value (DIV element):*/
                b.addEventListener("click", function (e) {
                    /*insert the value for the autocomplete text field:*/
                    inp.value = this.getElementsByTagName("input")[0].value;
                    //alert(inp.value);
                    doGetDates();
                    /*close the list of autocompleted values,
                    (or any other open lists of autocompleted values:*/
                    closeAllLists();
                });
                a.appendChild(b);
            }
        }
    });
    /*execute a function presses a key on the keyboard:*/
    inp.addEventListener("keydown", function (e) {
        var x = document.getElementById(this.id + "autocomplete-list");
        if (x) x = x.getElementsByTagName("div");
        if (e.keyCode == 40) {
            /*If the arrow DOWN key is pressed,
            increase the currentFocus variable:*/
            currentFocus++;
            /*and and make the current item more visible:*/
            addActive(x);
        } else if (e.keyCode == 38) { //up
            /*If the arrow UP key is pressed,
            decrease the currentFocus variable:*/
            currentFocus--;
            /*and and make the current item more visible:*/
            addActive(x);
        } else if (e.keyCode == 13) {
            /*If the ENTER key is pressed, prevent the form from being submitted,*/
            e.preventDefault();
            if (currentFocus > -1) {
                /*and simulate a click on the "active" item:*/
                if (x) x[currentFocus].click();
            }
        }
    });
    function addActive(x) {
        /*a function to classify an item as "active":*/
        if (!x) return false;
        /*start by removing the "active" class on all items:*/
        removeActive(x);
        if (currentFocus >= x.length) currentFocus = 0;
        if (currentFocus < 0) currentFocus = (x.length - 1);
        /*add class "autocomplete-active":*/
        x[currentFocus].classList.add("autocomplete-active");
    }
    function removeActive(x) {
        /*a function to remove the "active" class from all autocomplete items:*/
        for (var i = 0; i < x.length; i++) {
            x[i].classList.remove("autocomplete-active");
        }
    }
    function closeAllLists(elmnt) {
        /*close all autocomplete lists in the document,
        except the one passed as an argument:*/
        var x = document.getElementsByClassName("autocomplete-items");
        for (var i = 0; i < x.length; i++) {
            if (elmnt != x[i] && elmnt != inp) {
                x[i].parentNode.removeChild(x[i]);
            }
        }
    }
    /*execute a function when someone clicks in the document:*/
    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}

function GetStadiumInfo() {
    var url = "https://localhost:44397/uploader/GetStadiumNames";
    $.ajax({
        method: "GET",
        dataType: "json",
        url: url,
        async: false,
        context: document.body,
            success: function(data) {
                StadiumData = data;
            }
    });  
}

function checkStadiumNameExistOrNot(stadiumName)
{
    if (jQuery.inArray(stadiumName, StadiumData) === -1)
        return false;
    else
        return true;

}

function doGetDates()
{
    var stName = $("#myInput").val();
    var url = "https://localhost:44397/uploader/GetDatesOfStadium?stdName=" + stName;
    $.ajax({
        method: "GET",
        dataType: "json",
        url: url,
        async: false,
        context: document.body,
        success: function (data) {
            var dates = data;
            if (dates.length > 0) {
                //clear the all the option values
                //then add new value
                $('#datetimedata')
                    .empty();

                for (var i = 0; i < dates.length; i++) {
                    $('#datetimedata')
                        .append(' <li style="cursor:pointer;">' + dates[i] + '</li>');
                }
                $(".dropdown .dropdown-menu li")[0].click();
                $('#spnDocMsg').hide();
                $("#menu1").prop('disabled', false);
                $("#submitbutton").prop('disabled', false);
            }
        }
    });
}