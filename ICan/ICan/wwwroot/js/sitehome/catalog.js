// SELECTPICKER

const selectPicker = document.querySelector('#lang-selectpicker');
let selectedLang;

if (selectPicker) {

    const optionsTags = selectPicker.querySelectorAll('option');
    const options = [];

    for (let i = 0; i < optionsTags.length; i++) {
        options.push(optionsTags[i].getAttribute('name'))
    }

    const optionsMap = {
        'en': 'Тетради на английском',
        'ru': 'Тетради на русском'
    }
    
    selectedLang = parseUrl(window.location.search).notebookLang;
    if (selectedLang) {
        $('#lang-selectpicker').val(optionsMap[selectedLang])
    }
    
    $('#lang-selectpicker').on('changed.bs.select', function (e, clickedIndex) {
        selectedLang = options[clickedIndex];
        sendRequestOnSelectLanguage();
    })
    
    function sendRequestOnSelectLanguage() {
        if (!selectedLang) {
            return;
        } 
        window.location.href = `${window.siteUrls.filterProducts}?notebookLang=${selectedLang}`;
    }
}

/// FILTERS 
const series = document.querySelector(".filters-series");
const tags = document.querySelector(".filters-tag");



function onSelectSeries(e) {
    const target = e.target.closest(".filter-series");
    if (!target) {
        return;
    }
    let selectedSeriesId = target.id;
    if(target.classList.contains("selected")) {
        target.classList.remove("selected");
        selectedSeriesId = '';
    }
    sendRequestWithFilters(selectedSeriesId, undefined, selectedLang);
}

function onSelectTag(e) {
    const target = e.target.closest(".filter-tag");
    if (!target) {
        return;
    }
    let selectedTagId = target.id;
    if(target.classList.contains("selected")) {
        target.classList.remove("selected");
        selectedTagId = "";
    }
    
    sendRequestWithFilters(undefined, selectedTagId, selectedLang);
}

function getQueryForFiltersRequest(filter, tag, lang) {
    const queryParams = [];
    if (filter) { queryParams.push({ key: 'filter', value: parseInt(filter) }) }
    if (tag) { queryParams.push({ key: 'tag', value: parseInt(tag) }) }
    if (lang) {queryParams.push({ key: 'notebookLang', value: lang}) }

    let query = '';
    for (let i = 0; i < queryParams.length; i++) {
        if (i === 0) { query += '?'} else { query += '&' };
        query += `${queryParams[i].key}=${queryParams[i].value}`
    }
    return query;
}

// TESTS

// console.log("1: ", getQueryForFiltersRequest());
// console.log("2: ", getQueryForFiltersRequest("3-filter", undefined));
// console.log("3: ", getQueryForFiltersRequest(undefined, "2-tag"));
// console.log("4: ", getQueryForFiltersRequest(undefined, "2-tag", "en"));

function sendRequestWithFilters(filter, tag, lang) {
    const query = getQueryForFiltersRequest(filter, tag, lang);
    window.location.href = `${window.siteUrls.filterProducts}${query}`;
}

function parseUrl(url) {
    const params = {}
    const strings = url.slice(1).split("&");
    strings.forEach(param => {
        const keyvaluePair = param.split("=");
        params[keyvaluePair[0]] = keyvaluePair[1];
    })
    return params;
}

// TESTS

// console.log("1: ", parseUrl("?notebookLang=en"));
// console.log("2: ", parseUrl("?tag=1&notebookLang=en"));
// console.log("3: ", parseUrl("?tag=1&filter=2&notebookLang=en"));