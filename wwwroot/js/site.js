// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var limit = 4;
$('input.IsChecked').on('click', function(evt) {
    if ($('.IsChecked:checked').length > limit) {
       this.checked = false;
   }
});