window.historyBack = (fallbackUrl) => {
    if (history.length > 1) {
        history.back();
    } else {
        window.location.href = fallbackUrl;
    }
};
