﻿export function setCookie(key, value) {
    var cookies = document.cookie.split('; ');
    var isExist = false;
    for (var i = 0; i < cookies.length; i++) {
        var parts = cookies[i].split('=');
        if (parts[0] === key) {
            isExist = true;
            parts[1] = encodeURIComponent(value);
            document.cookie = parts.join('=');
            break;
        }
    }

    if (!isExist) {
        document.cookie = [key, encodeURIComponent(value)].join('=');
    }
}

export function getCookie(key) {
    var cookies = document.cookie.split('; ');
    var value = '';
    for (var i = 0; i < cookies.length; i++) {
        var parts = cookies[i].split('=');
        if (parts[0] === key) {
            value = decodeURIComponent(parts[1]);
            break;
        }
    }

    return value;
}