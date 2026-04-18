// ===================================
// MUA CHƯƠNG + ĐỔI NÚT THÀNH ĐỌC NGAY
// ===================================
async function muaChuong(maChuong) {
    const btn = event.target.closest('button');
    const cardLink = btn.closest('a');

    if (!btn) return;

    btn.disabled = true;

    const returnUrl = encodeURIComponent(window.location.href);

    try {
        const response = await fetch('/Truyen/MuaChuong', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: `maChuong=${encodeURIComponent(maChuong)}&returnUrl=${returnUrl}`
        });

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const result = await response.json();

        if (result.success) {
            showSuccess(result.message);

            // Redirect thẳng đến đọc chương (cho nút top + card)
            setTimeout(() => {
                window.location.href = '/Truyen/DocChuong/' + maChuong;
            }, 1000);

            // UI update nếu ở lại page
            if (cardLink) {
                cardLink.href = '/Truyen/DocChuong/' + maChuong;
                cardLink.removeAttribute('onclick');
            }
            btn.classList.remove('btn-outline-primary');
            btn.classList.add('btn-success');
            btn.innerHTML = '<i class="fas fa-play me-1"></i>Đọc ngay';
        } else {
            showError(result.message || 'Lỗi không xác định!');
        }

    } catch (error) {
        console.error('Mua chương error:', error);
        showError('Lỗi hệ thống. Vui lòng thử lại! Kiểm tra console F12.');
        console.log('Full response error:', error);
    } finally {
        btn.disabled = false;
        if (!btn.classList.contains('btn-success')) {
            btn.innerHTML = '<i class="fas fa-lock me-1"></i>Mua & Đọc';
        }
    }
}


// ===================================
// TOAST SUCCESS
// ===================================
function showSuccess(msg) {

    const toast = document.createElement('div');

    toast.className =
        'toast-success position-fixed top-0 end-0 m-3 p-3 rounded shadow';

    toast.innerHTML = `
        <i class="fas fa-check-circle me-2"></i>${msg}
    `;

    document.body.appendChild(toast);

    setTimeout(() => toast.remove(), 2500);
}


// ===================================
// TOAST ERROR
// ===================================
function showError(msg) {

    const toast = document.createElement('div');

    toast.className =
        'toast-error position-fixed top-0 end-0 m-3 p-3 rounded shadow';

    toast.innerHTML = `
        <i class="fas fa-times-circle me-2"></i>${msg}
    `;

    document.body.appendChild(toast);

    setTimeout(() => toast.remove(), 3000);
}


// ===================================
// CSS AUTO
// ===================================
(function () {

    const style = document.createElement('style');

    style.innerHTML = `
        .toast-success{
            background:#198754;
            color:#fff;
            z-index:9999;
            min-width:220px;
        }

        .toast-error{
            background:#dc3545;
            color:#fff;
            z-index:9999;
            min-width:220px;
        }

        .card{
            transition:0.25s;
        }

        .card:hover{
            transform:translateY(-5px);
        }
    `;

    document.head.appendChild(style);

})();

console.log('Mua chương PRO loaded!');