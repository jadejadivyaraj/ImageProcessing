 $(document).ready(function () {
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
                    $('#btnSubmit').prop('disabled', true);
                }
                else {
                    // Check and restrict the file size to 10MB.
                    if ($(this).get(0).files[0].size > (10048576)) {
                        $('#spnDocMsg').text("Sorry!! Max allowed file size is 10 MB").show();                
                     // Clear fileuload control selected file
                        $(this).replaceWith($(this).val('').clone(true));
                        //Disable Submit Button
                        $('#btnSubmit').prop('disabled', true);
                    }
                    else {
                        //Clear and Hide message span
                        $('#spnDocMsg').text('').hide();
                        //Enable Submit Button
                        $('#btnSubmit').prop('disabled', false);
                    }
                }
            });
        });