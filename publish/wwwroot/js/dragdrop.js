// Download file helper (called from Blazor for Excel export)
window.downloadFile = function (filename, base64) {
    const link = document.createElement('a');
    link.href = 'data:application/octet-stream;base64,' + base64;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Visual feedback for drag-drop on the scheduling board
document.addEventListener('dragover', function (e) {
    const slot = e.target.closest('.time-slot');
    if (slot) {
        e.preventDefault();
        slot.classList.add('drag-over');
    }
});

document.addEventListener('dragleave', function (e) {
    const slot = e.target.closest('.time-slot');
    if (slot) slot.classList.remove('drag-over');
});

document.addEventListener('drop', function (e) {
    document.querySelectorAll('.drag-over').forEach(el => el.classList.remove('drag-over'));
});
