// site.js - JavaScript functionality for Slottet

// jQuery ready function
$(document).ready(function () {
    console.log('Slottet system loaded');
    
    // Auto-hide alerts after 5 seconds
    setTimeout(function() {
        $('.alert').fadeOut('slow');
    }, 5000);
    
    // Confirm delete actions
    $('.delete-confirm').on('click', function(e) {
        if (!confirm('Er du sikker på at du vil slette dette?')) {
            e.preventDefault();
        }
    });
});

// EKSEMPLER PÅ FUNKTIONER MAN KUNNE TILFØJE:

// 1. AJAX form submission
function submitForm(formId, successCallback) {
    var form = $('#' + formId);
    $.ajax({
        url: form.attr('action'),
        type: form.attr('method'),
        data: form.serialize(),
        success: function(response) {
            if (successCallback) successCallback(response);
        },
        error: function(xhr) {
            alert('Der opstod en fejl: ' + xhr.statusText);
        }
    });
}

// 2. Search med debounce
var searchTimer;
function performSearch(searchTerm) {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(function() {
        $.get('/api/residents/search?term=' + searchTerm, function(results) {
            // Update UI with results
            displaySearchResults(results);
        });
    }, 300); // Wait 300ms after user stops typing
}

// 3. Modal dialogs
function showModal(title, content, buttons) {
    var modal = $('<div class="modal fade" tabindex="-1">');
    var dialog = $('<div class="modal-dialog">');
    var content = $('<div class="modal-content">');
    
    content.append('<div class="modal-header"><h5 class="modal-title">' + title + '</h5></div>');
    content.append('<div class="modal-body">' + content + '</div>');
    content.append('<div class="modal-footer">' + buttons + '</div>');
    
    dialog.append(content);
    modal.append(dialog);
    $('body').append(modal);
    modal.modal('show');
}

// 4. Toast notifications
function showToast(message, type = 'info') {
    var toastClass = 'bg-' + type;
    var toast = $('<div class="toast ' + toastClass + '" role="alert">');
    toast.html('<div class="toast-body">' + message + '</div>');
    
    $('.toast-container').append(toast);
    toast.toast('show');
}

// 5. Print functionality
function printSection(sectionId) {
    var content = $('#' + sectionId).html();
    var printWindow = window.open('', '', 'height=600,width=800');
    
    printWindow.document.write('<html><head><title>Print</title>');
    printWindow.document.write('<link rel="stylesheet" href="/css/site.css" />');
    printWindow.document.write('</head><body>');
    printWindow.document.write(content);
    printWindow.document.write('</body></html>');
    
    printWindow.document.close();
    printWindow.print();
}

// 6. Form validation helpers
function validateCPR(cpr) {
    if (!cpr || cpr.length !== 10) return false;
    // Add CPR validation logic
    return /^\d{10}$/.test(cpr);
}

function extractBirthDateFromCPR(cpr) {
    if (!validateCPR(cpr)) return null;
    
    var day = cpr.substring(0, 2);
    var month = cpr.substring(2, 4);
    var year = cpr.substring(4, 6);
    
    // Determine century
    var fullYear = parseInt(year) > 20 ? '19' + year : '20' + year;
    
    return fullYear + '-' + month + '-' + day;
}

// 7. Autosave functionality
var autosaveTimer;
function enableAutosave(formId, interval = 30000) {
    autosaveTimer = setInterval(function() {
        var formData = $('#' + formId).serialize();
        localStorage.setItem('autosave_' + formId, formData);
        console.log('Form autosaved');
    }, interval);
}

function loadAutosave(formId) {
    var saved = localStorage.getItem('autosave_' + formId);
    if (saved && confirm('Der findes gemt data. Vil du gendanne?')) {
        $('#' + formId).deserialize(saved);
    }
}

// 8. Export table to CSV
function exportTableToCSV(tableId, filename) {
    var csv = [];
    var rows = $('#' + tableId + ' tr');
    
    for (var i = 0; i < rows.length; i++) {
        var row = [], cols = rows[i].querySelectorAll('td, th');
        
        for (var j = 0; j < cols.length; j++) {
            row.push(cols[j].innerText);
        }
        
        csv.push(row.join(','));
    }
    
    downloadCSV(csv.join('\n'), filename);
}

function downloadCSV(csv, filename) {
    var csvFile = new Blob([csv], { type: 'text/csv' });
    var downloadLink = document.createElement('a');
    
    downloadLink.download = filename;
    downloadLink.href = window.URL.createObjectURL(csvFile);
    downloadLink.style.display = 'none';
    
    document.body.appendChild(downloadLink);
    downloadLink.click();
}
