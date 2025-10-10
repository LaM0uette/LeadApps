(function(){
  const state = {
    handler: null,
    target: null,
    loading: false,
    lastInvokeTs: 0,
    throttleMs: 200
  };

  window.TopDeck = window.TopDeck || {};

  // Register infinite scroll on either a specific container (by CSS selector) or the window.
  // Usage:
  //   registerInfiniteScroll(dotnetRef, threshold)
  //   registerInfiniteScroll(dotnetRef, selector, threshold)
  window.TopDeck.registerInfiniteScroll = function(dotnetRef, selectorOrThreshold, maybeThreshold){
    const hasSelector = typeof selectorOrThreshold === 'string';
    const target = hasSelector ? (document.querySelector(selectorOrThreshold) || window) : window;
    const th = hasSelector ? (typeof maybeThreshold === 'number' ? maybeThreshold : 300) : (typeof selectorOrThreshold === 'number' ? selectorOrThreshold : 300);

    const handler = function(){
      const el = (target && target !== window) ? target : null;
      let scrollTop, viewport, fullHeight;
      if (el){
        scrollTop = el.scrollTop || 0;
        viewport = el.clientHeight || 0;
        fullHeight = el.scrollHeight || 0;
      } else {
        scrollTop = window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop || 0;
        viewport = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight || 0;
        fullHeight = Math.max(document.body.scrollHeight, document.documentElement.scrollHeight);
      }
      const nearBottom = (scrollTop + viewport) >= (fullHeight - th);
      if (!nearBottom) return;
      const now = (window.performance && performance.now) ? performance.now() : Date.now();
      if (state.loading) return;
      if (now - state.lastInvokeTs < state.throttleMs) return;
      state.lastInvokeTs = now;
      try{
        state.loading = true;
        Promise.resolve(dotnetRef.invokeMethodAsync('OnNearBottom'))
          .catch(function(){})
          .finally(function(){ state.loading = false; });
      }catch(e){
        state.loading = false;
      }
    };

    // Remove previous
    if (state.handler){
      const prevTarget = state.target || window;
      prevTarget.removeEventListener('scroll', state.handler);
    }

    state.handler = handler;
    state.target = target;
    (target || window).addEventListener('scroll', handler, { passive: true });

    // Run an initial check to preload if we're already near the bottom
    try { handler(); } catch(e) {}
  };

  // Returns true if the content is taller than the viewport (can scroll)
  // Usage:
  //   canScroll(threshold)
  //   canScroll(selector, threshold)
  window.TopDeck.canScroll = function(selectorOrThreshold, maybeThreshold){
    const hasSelector = typeof selectorOrThreshold === 'string';
    const el = hasSelector ? document.querySelector(selectorOrThreshold) : null;
    const th = hasSelector ? (typeof maybeThreshold === 'number' ? maybeThreshold : 0) : (typeof selectorOrThreshold === 'number' ? selectorOrThreshold : 0);

    if (el){
      const viewport = el.clientHeight || 0;
      const fullHeight = el.scrollHeight || 0;
      return (fullHeight - th) > viewport;
    } else {
      const viewport = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight || 0;
      const fullHeight = Math.max(document.body.scrollHeight, document.documentElement.scrollHeight);
      return (fullHeight - th) > viewport;
    }
  };

  window.TopDeck.unregisterInfiniteScroll = function(){
    if (state.handler){
      const target = state.target || window;
      target.removeEventListener('scroll', state.handler);
      state.handler = null;
      state.target = null;
    }
  };
})();
