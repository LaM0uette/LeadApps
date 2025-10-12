let resizeHandler = null;

window.registerResizeHandler = (dotnetHelper) => {
    // on supprime d'abord tout ancien handler
    if (resizeHandler) {
        window.removeEventListener("resize", resizeHandler);
    }

    resizeHandler = () => {
        dotnetHelper.invokeMethodAsync("OnResize", {
            width: window.innerWidth,
            height: window.innerHeight
        });
    };

    window.addEventListener("resize", resizeHandler);
};

window.unregisterResizeHandler = () => {
    if (resizeHandler) {
        window.removeEventListener("resize", resizeHandler);
        resizeHandler = null;
    }
};

window.getWindowSize = () => {
    return {
        width: window.innerWidth,
        height: window.innerHeight
    };
};