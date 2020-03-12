function alertModal(content, seconds, title) {
    if (seconds === undefined) {
        seconds = 1000;
    }
    if (title === undefined) {
        title = "Notice";
    }
    $('#alertModal').modal('show');
    $('#alertModal .title').text(title);
    $('#alertModal .modal-body').html(content);
    if (seconds > 0) {
        $('#alertModal .countDown').text("(close after " + seconds / 1000 + "s)");
        setTimeout(function () { $('#alertModal').modal('hide'); }, seconds);
    }
}

function convertStrToObject(str) {
    var ret = {};
    var query = str.split("&");
    for (var i = 0; i < query.length; i++) {
        var j = query[i].indexOf("=");
        var value = query[i].substring(j + 1);
        if (value === "null") {
            value = null;
        }
        else if (value === "true") {
            value = true;
        }
        else if (value === "false") {
            value = false;
        }
        else if (Number(value) && parseFloat(value)) {
            value = parseFloat(value);
        }
        ret[query[i].substring(0, j).toLowerCase()] = value;
    }
    return ret;
}
function convertHashToObject(str) {
    if (typeof str === 'undefined') {
        str = window.location;
    }
    str = decodeURI(str);
    var index = str.indexOf('#');
    return convertStrToObject(str.substr(index + 1));
}
function convertObjectToQuery(object) {
    var array = [];
    for (var key in object) {
        if (object.hasOwnProperty(key)) {
            array.push(key+'='+object[key]);
        }
    }
    return array.join('&');
}
function convertQueryToObject(str) {
    var ret = {};
    if (typeof str === 'undefined') {
        str = window.location;
    }
    str = decodeURI(str);
    var index = str.indexOf('?');
    return convertStrToObject(str.substr(index + 1));
}