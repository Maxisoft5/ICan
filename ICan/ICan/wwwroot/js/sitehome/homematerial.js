const footerMargin = -80;

function scrollMaterilal() {
    const hash = window.location.hash;
    if(hash) {
        window.scrollBy(0, footerMargin);
    }else {
        return;
    }
}


window.onload = function() {
    scrollMaterilal();
}