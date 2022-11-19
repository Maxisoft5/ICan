const carouselPopup = $('#carousel-popup');
const videoWrapper = $(".video-wrapper");
const carouselProductsInner = $("#carousel-products .carousel-inner");
const productsPopup = $('#products-popup');
const carouselPopupInner = $("#carousel-popup .carousel-inner");
const carouselProduct = $('#carousel-products');
const modalContent = document.querySelector('.modal-content');
const tabPanes = document.querySelectorAll('.tab-pane');
const video = document.querySelector('.video');
const closeModal = modalContent.querySelector('.cross-close');

function stopVideo() {
    video.pause();
    video.currentTime = 0;
}

closeModal.addEventListener('click',stopVideo);


function isLarge() {
    const largeVisible = document.querySelector('.is-large');
    return !!largeVisible.offsetWidth;
}

const isDesktop = isLarge();

function dropDownHiddenOnMobile() {
    if(isDesktop) {
        return;
    }
    for(let i=0; i < tabPanes.length; i++) {
        const dropDown = tabPanes[i].querySelector('.drop-down');
        if(!tabPanes[i].querySelector('.tab-hidden')) {
            dropDown.classList.remove('closed');
            dropDown.classList.add('opened');
        }
    }
}

dropDownHiddenOnMobile();

carouselProduct.carousel({
    interval: null
  })
  
carouselProduct.swipe({
    swipe: function(
        event,
        direction
    ) {
    if (direction == "left") $(this).carousel("next");
    if (direction == "right") $(this).carousel("prev");
    },
    allowPageScroll: "vertical",
    preventDefaultEvents: false
});



carouselProductsInner.click(function(e) {
    const slide = e.target.closest(".item");
    if (slide) {
        if (slide.dataset.video) {
            openVideo();
            return;
        }
        const selected_slide = +slide.dataset.number;
        carouselPopup.carousel(selected_slide)
        carouselPopup.carousel('pause')
        carouselPopup.swipe({
            swipe: function(
                event,
                direction
            ) {
            if (direction == "left") $(this).carousel("next");
            if (direction == "right") $(this).carousel("prev");
            },
            allowPageScroll: "vertical"
        });
        productsPopup.modal({})
    }
});

productsPopup.on('hidden.bs.modal', function (e) {
    const isVideoOpened = !videoWrapper.hasClass("hidden");
    if (isVideoOpened) {
        hideVideo();
    }
})

carouselPopupInner.click(function(e) {
    const slide = e.target.closest(".item");
    if (slide && slide.dataset.video) {
        openVideo();
    }
});


const dropDown = document.querySelectorAll('.drop-down');


function dropDownHidden(number) {
    dropDown[number].classList.remove('closed');
    dropDown[number].classList.add('opened');
}

for(let i=0; i<dropDown.length; i++) {
    dropDown[i].onclick = function() {
        const tabPane = dropDown[i].closest('.tab-pane');
        const tabHidden = tabPane.querySelector('.tab-hidden')
        tabHidden.classList.add('tab-show');
        dropDownHidden(i);
    }
}

function openVideo() {
    carouselPopup.addClass("hidden");
    videoWrapper.removeClass("hidden");
    video.play();
    $('#products-popup').modal({});
}

function hideVideo() {
    videoWrapper.addClass("hidden");
    carouselPopup.removeClass("hidden");
    
}

const videoIcons = document.querySelectorAll(".video-icon-circle");
for (let i = 0; i < videoIcons.length; i++) {
    videoIcons[i].onclick = openVideo;
}

function zoom(e){
    if(!isDesktop) {
        return;
    }
    let zoomer = e.currentTarget;
    let img = zoomer.querySelector('img');
    if (!img) {
        return;
    }
    img.style.opacity = 0;
    let src = img.getAttribute('src');
    e.offsetX ? offsetX = e.offsetX : 0
    e.offsetY ? offsetY = e.offsetY : 0
    x = offsetX/zoomer.offsetWidth*100
    y = offsetY/zoomer.offsetHeight*100
    zoomer.style.backgroundPosition = x + '% ' + y + '%';
    zoomer.style.backgroundImage = `url(${src})`;
}

function clearBackground(e) {
    let zoomer = e.currentTarget;
    zoomer.style.backgroundImage = '';
    let img = zoomer.querySelector('img');
    if (img) {
        img.style.opacity = 1;
    }
}

modalContent.onclick = function(e) {
    let target = e.target;
    let indicatorWrapper = target.closest('.indicator-wrapper');
    let isnotebookImage = target.closest('.zoom img');
    let isArrowSwipe = target.closest('.arrow-swipe');
    let isVideo = target.closest('.video');
    if(isnotebookImage || isArrowSwipe || indicatorWrapper || isVideo) {
        return;
    }
    stopVideo();
    productsPopup.modal('hide');
}

