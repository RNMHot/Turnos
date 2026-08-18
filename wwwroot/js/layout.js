// Reports whether the viewport is at or below the given width (used to default the sidebar collapsed on phones/tablets)
window.isSmallScreen = function (maxWidth) {
    return window.innerWidth <= maxWidth;
};

// Scrolls the week grid horizontally so today's column lands right after the pinned time column
window.scrollWeekGridToToday = function () {
    var wrapper = document.querySelector('.week-grid-wrapper');
    if (!wrapper) return;

    var today = wrapper.querySelector('.day-header.hoy');
    var corner = wrapper.querySelector('.grid-corner');
    wrapper.scrollLeft = (today && corner) ? (today.offsetLeft - corner.offsetWidth) : 0;
};
