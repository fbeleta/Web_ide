// ═══════════════════════════════════════════════════════════════
//  site.js — WebIde Lab 4 JavaScript
//  AJAX search, animations, blur validation, autocomplete helpers
// ═══════════════════════════════════════════════════════════════

// ── 1. CLIENT-SIDE BLUR VALIDATION ──────────────────────────────
$(document).on('blur', 'input[data-val], select[data-val], textarea[data-val]', function () {
    var form = $(this).closest('form');
    if (form.length && form.data('validator')) {
        form.validate().element(this);
    }
});

// ── 2. PAGE-LOAD ROW STAGGER ANIMATION ──────────────────────────
$(function () {
    var rows = document.querySelectorAll('tbody tr');
    rows.forEach(function (row, i) {
        row.style.opacity = '0';
        row.style.transform = 'translateY(8px)';
        row.style.transition = 'opacity 0.18s ease, transform 0.18s ease';
        setTimeout(function () {
            row.style.opacity = '1';
            row.style.transform = 'translateY(0)';
        }, 40 + i * 35);
    });
});

// ── 3. AJAX SEARCH HELPER ────────────────────────────────────────
/**
 * initAjaxSearch(inputId, tbodyId, endpoint, rowRenderer)
 *
 * inputId     — id of the search <input>
 * tbodyId     — id of the <tbody> to replace
 * endpoint    — URL string (e.g. '/tags/search')
 * rowRenderer — function(item) => HTML string for a <tr>
 */
function initAjaxSearch(inputId, tbodyId, endpoint, rowRenderer) {
    var input = document.getElementById(inputId);
    var tbody = document.getElementById(tbodyId);
    if (!input || !tbody) return;

    var timer;
    input.addEventListener('keyup', function () {
        clearTimeout(timer);
        var q = this.value.trim();
        timer = setTimeout(function () {
            // Show skeleton
            showSearchSkeleton(tbody);

            fetch(endpoint + '?q=' + encodeURIComponent(q))
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    tbody.innerHTML = data.map(rowRenderer).join('');
                    // Stagger new rows
                    var newRows = tbody.querySelectorAll('tr');
                    newRows.forEach(function (row, i) {
                        row.style.opacity = '0';
                        row.style.transform = 'translateY(6px)';
                        row.style.transition = 'opacity 0.15s ease, transform 0.15s ease';
                        setTimeout(function () {
                            row.style.opacity = '1';
                            row.style.transform = 'translateY(0)';
                        }, i * 40);
                    });
                })
                .catch(function () {
                    tbody.innerHTML = '<tr><td colspan="99" class="text-center py-4 text-error font-bold text-xs uppercase tracking-widest" style="font-family:\'Space Grotesk\',sans-serif;">SEARCH FAILED</td></tr>';
                });
        }, 300);
    });
}

function showSearchSkeleton(tbody) {
    var skeletonRow = '<tr>' +
        Array(5).fill('<td><div class="h-4 bg-[#e0e0e0] animate-pulse w-full"></div></td>').join('') +
        '</tr>';
    tbody.innerHTML = skeletonRow + skeletonRow + skeletonRow;
}

// ── 4. FORM FOCUS ANIMATION ──────────────────────────────────────
$(document).on('focus', '.brutalist-input', function () {
    $(this).css('border-width', '3px');
}).on('blur', '.brutalist-input', function () {
    $(this).css('border-width', '2px');
});

// ── 5. FLASH NOTIFICATION AUTO-DISMISS ──────────────────────────
$(function () {
    var flash = document.getElementById('flash-message');
    if (flash) {
        setTimeout(function () {
            flash.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
            flash.style.opacity = '0';
            flash.style.transform = 'translateY(-10px)';
            setTimeout(function () { flash.remove(); }, 420);
        }, 3500);
    }
});
