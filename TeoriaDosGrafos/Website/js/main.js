var submitInfo;
var page;
var message_max = 25;

$(document).ready(function(){

	changePage = function(id) {
		if (id == page) return;

		$('.active').hide();
		$('.active').removeClass('active');

		var toActivate = (id != null) ? id : page;
		page = toActivate;

		$(toActivate).show();

	    var totop = setInterval(function () {
	      $(".pages").animate({scrollTop: 0}, 0);
	    }, 1);

	  setTimeout(function () {
	      $(toActivate).addClass('active');
	      setTimeout(function () {
	        clearInterval(totop);
	      }, 1000);
	    }, 100);
	}

    changePage('#novoGrafo');
	$('.page-button').click(function(){changePage($(this).data('target-page'))})

    $.material.init();

    $(document).on('click', 'div[class^="item-"] .btn-add', function () {
        var $item = $(this).parents('div[class^="item-"]');
        $item.after($item.clone().find('input[type!="hidden"]').val('').end());
    });

    $(document).on('click', 'div[class^="item-"] .btn-remove', function () {
        var $item = $(this).parents('div[class^="item-"]');
        $item.remove();
    });

})

var getQueryString = function (form) {
    var inputs = $(form).serializeArray();
    var queryString = '';

    var i = 0;
    while (i < inputs.length) {
        if (i != 0)
            queryString += '&';
        queryString += inputs[i].value;
        queryString += '={';

        var j = i + 1;
        while ((j < inputs.length) && inputs[j].name != 'tipo') {
            queryString += inputs[j].name;
            queryString += ':\'';
            queryString += inputs[j].value;
            queryString += '\'';

            if ((j + 1 < inputs.length) && (inputs[j + 1].name != 'tipo'))
                queryString += ', ';

            ++j;
        }

        queryString += '}';
        i = j;
    }
    
    return queryString;
}

var novoGrafo = function () {
    var form = '#novoGrafo form';
    var queryString = getQueryString(form);

    api.grafo.post(queryString);
    return false;
}

var lerGrafo = function () {
    api.grafo.get();
}

