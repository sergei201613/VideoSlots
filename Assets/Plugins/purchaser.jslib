var IframePlugin = {
      jsOpenClick: function() {
      	var ovelay = document.getElementById('overlayad');
        if(ovelay) ovelay.style.display = 'block';
      },

      jsCloseClick: function() {
      	var ovelay = document.getElementById('overlayad');
        if(ovelay) ovelay.style.display = 'none';      
      },

      jsRefreshClick: function(){
      	document.getElementById('ifads').contentWindow.location.reload();
      }
    };
    mergeInto(LibraryManager.library, IframePlugin);