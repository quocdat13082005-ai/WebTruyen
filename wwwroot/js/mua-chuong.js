// ===================================
// MUA CHUONG + DOI NUT THANH DOC NGAY
// ===================================
async function muaChuong(maChuong) {
    const activeElement = document.activeElement;
    const btn = activeElement instanceof HTMLElement ? activeElement.closest('button') : null;
    const fallbackButton = document.querySelector(`button[onclick*="muaChuong(${maChuong})"]`);
    const targetButton = btn || fallbackButton;

    if (!targetButton) return;

    const cardLink = targetButton.closest('a');
    targetButton.disabled = true;

    const returnUrl = encodeURIComponent(window.location.href);

    try {
        const response = await fetch('/MuaChuong/MuaChuong', {
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

            setTimeout(() => {
                window.location.href = '/Truyen/DocChuong/' + maChuong;
            }, 1000);

            if (cardLink) {
                cardLink.href = '/Truyen/DocChuong/' + maChuong;
                cardLink.removeAttribute('onclick');
            }

            targetButton.classList.remove('btn-outline-primary');
            targetButton.classList.add('btn-success');
            targetButton.innerHTML = '<i class="fas fa-play me-1"></i>Doc ngay';
        } else {
            showError(result.message || 'Loi khong xac dinh.');
        }
    } catch (error) {
        console.error('Mua chuong error:', error);
        showError('Loi he thong. Vui long thu lai.');
    } finally {
        targetButton.disabled = false;

        if (!targetButton.classList.contains('btn-success')) {
            targetButton.innerHTML = '<i class="fas fa-lock me-1"></i>Mua & Doc';
        }
    }
}

function showSuccess(msg) {
    const toast = document.createElement('div');
    toast.className = 'toast-success position-fixed top-0 end-0 m-3 p-3 rounded shadow';
    toast.innerHTML = `<i class="fas fa-check-circle me-2"></i>${msg}`;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 2500);
}

function showError(msg) {
    const toast = document.createElement('div');
    toast.className = 'toast-error position-fixed top-0 end-0 m-3 p-3 rounded shadow';
    toast.innerHTML = `<i class="fas fa-times-circle me-2"></i>${msg}`;
    document.body.appendChild(toast);
    setTimeout(() => toast.remove(), 3000);
}

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

console.log('Mua chuong JS loaded');
