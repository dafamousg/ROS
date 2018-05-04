﻿var geocoder;
var map;
var marker; // Define marker here so we don't have more than one.

function initialize() {
    geocoder = new google.maps.Geocoder();
    var latlng = new google.maps.LatLng(57.678691, 12.001445);
    var mapOptions = {
        zoom: 8,
        center: latlng
    }
    map = new google.maps.Map(document.getElementById('map'), mapOptions);

    // Enable autocomplete
    var input = document.getElementById('Address');
    var options = {
        types: [],
        componentRestrictions: {country: 'se'}
    };

    var autocomplete = new google.maps.places.Autocomplete(input, options);
}

function codeAddress() {
    var address = document.getElementById('Address').value;
    geocoder.geocode({ 'address': address }, function (results, status) {
        if (status == 'OK') {
            map.setCenter(results[0].geometry.location);
            map.setZoom(15);

            if (marker) {
                marker.setPosition(results[0].geometry.location);
            }
            else {
                marker = new google.maps.Marker({
                    map: map,
                    position: results[0].geometry.location
                });
            }

        } else {
            alert('Geocode was not successful for the following reason: ' + status);
        }
    });
}
