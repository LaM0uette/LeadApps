(function(){
  const state = {
    handler: null
  };

  window.TopDeck = window.TopDeck || {};

  window.TopDeck.registerInfiniteScroll = function(dotnetRef, threshold){
    const th = typeof threshold === 'number' ? threshold : 300;
    const handler = function(){
      const scrollTop = window.pageYOffset || document.documentElement.scrollTop || document.body.scrollTop || 0;
      const viewport = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight || 0;
      const fullHeight = Math.max(document.body.scrollHeight, document.documentElement.scrollHeight);
      if (scrollTop + viewport >= fullHeight - th){
        // debounced by awaiting .NET side loading flag
        try{ dotnetRef.invokeMethodAsync('OnNearBottom'); } catch(e){}
      }
    };
    if (state.handler){
      window.removeEventListener('scroll', state.handler);
    }
    state.handler = handler;
    window.addEventListener('scroll', handler, { passive: true });
  };

  // Returns true if the page content is taller than the viewport (can scroll)
  window.TopDeck.canScroll = function(threshold){
    const th = typeof threshold === 'number' ? threshold : 0;
    const viewport = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight || 0;
    const fullHeight = Math.max(document.body.scrollHeight, document.documentElement.scrollHeight);
    return (fullHeight - th) > viewport;
  };

  window.TopDeck.unregisterInfiniteScroll = function(){
    if (state.handler){
      window.removeEventListener('scroll', state.handler);
      state.handler = null;
    }
  };
})();
