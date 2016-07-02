
var source = "Apne";

function getChannel() {

    setContentDiv();

    if (getUrlVars()["source"] != null)
        source = unescape(getUrlVars()["source"]);

    var script = "http://mytvweb.azurewebsites.net/GetShow.aspx";
    $.getScript(script, function (data, textStatus, jqxhr) {
        var obj = myTVScript.serials.filter(function (e) { return e.Source == source });
        var ch = obj[0].Channel;
        $.each(ch, function (i, c) {

            if (i != 0) {
                var od = $('#div-0');
                var cd = $(od).clone();

                $(cd).attr('id', 'div-' + i);
                cd.html(cd.html().replace('a-0', 'a-' + i).replace('span-0', 'span-' + i).replace('ui-btn-icon-right', '').replace('ui-corner-top', ''));

                $(od).parent().append(cd);
            }

            $('#a-' + i).attr('href', 'show.html?source=' + source + '&s=' + escape(c.Name));
            $('#span-' + i).html(c.Name);
        });

        HideProgress('content');
    });
}

function getChannel1() {

    setContentDiv();

    if (getUrlVars()["source"] != null)
        source = unescape(getUrlVars()["source"]);

    var script = "http://mytvweb.azurewebsites.net/GetShow.aspx";
    $.getScript(script, function (data, textStatus, jqxhr) {
        var obj = myTVScript.serials.filter(function (e) { return e.Source == source });
        var ch = obj[0].Channel;
        $.each(ch, function (i, c) {

            var h3 = $('<h3>');
            $(h3).html(c.Name);
            $('#accordion').append(h3);

            var div = $('<div>');
            $(div).append($('<button style="float:left">&lt;</button>'));
            var d1 = $('<div class="scroll-pane ui-widget ui-widget-header ui-corner-all">');
            var d2 = $('<div class="scroll-content">');
            $.each(c.Shows, function (j, s) {

                var a = $('<a href="date.html?source=' + source + '&s=' + escape(s.Name) + '&url=' + escape(s.URL) + '">' + s.Name + '</a>');
                var show = $('<div class="scroll-content-item ui-widget-header">');
                $(show).append(a);
                $(d2).append(show);
            });
            $(d2).css('width', c.Shows.length * 180);
            $(d1).append(d2);
            $(div).append(d1);
            $(div).append($('<button style="float:left">&gt;</button>'));
            $('#accordion').append(div);

        });

        $("#accordion").accordion({
            event: "click hoverintent",
            heightStyle: "content"
        });

        HideProgress('content');
    });
}

function getShow() {

    setContentDiv();

    source = unescape(getUrlVars()["source"]);
    var show = unescape(getUrlVars()["s"]);

    var script = "http://mytvweb.azurewebsites.net/GetShow.aspx";
    $.getScript(script, function (data, textStatus, jqxhr) {

        var obj = myTVScript.serials.filter(function (e) { return e.Source == source });
        var sh = obj[0].Channel.filter(function (e) { return e.Name == show });

        $.each(sh[0].Shows, function (i, s) {

            if (i != 0) {
                var od = $('#div-0');
                var cd = $(od).clone();

                $(cd).attr('id', 'div-' + i);
                cd.html(cd.html().replace('a-0', 'a-' + i).replace('span-0', 'span-' + i).replace('ui-btn-icon-right', '').replace('ui-corner-top', ''));

                $(od).parent().append(cd);
            }

            $('#a-' + i).attr('href', 'date.html?source=' + source + '&s=' + escape(s.Name) + '&url=' + escape(s.URL));
            $('#span-' + i).html(s.Name);
        });
        HideProgress('content');
    });
}

function getDate() {

    setContentDiv();

    var show = getUrlVars()["s"];
    var url = getUrlVars()["url"];
    source = getUrlVars()["source"];

    var script = "http://mytvweb.azurewebsites.net/GetDate.aspx?s=" + show;
    $.getScript(script, function (data, textStatus, jqxhr) {

        $.each(myTVScript.dates, function (index, elem) {

            if (index != 0) {
                var od = $('#div-0');
                var cd = $(od).clone();

                $(cd).attr('id', 'div-' + index);
                cd.html(cd.html().replace('a-0', 'a-' + index).replace('span-0', 'span-' + index).replace('ui-btn-icon-right', '').replace('ui-corner-top', ''));

                $(od).parent().append(cd);
            }

            $('#a-' + index).attr('href', 'video.html?source=' + source + '&s=' + show + '&d=' + elem + '&url=' + url);
            $('#span-' + index).html(elem);
        });
        HideProgress('content');
    });
}

function getFormat() {

    setContentDiv();

    var show = getUrlVars()["s"];
    var date = getUrlVars()["d"];
    var url = getUrlVars()["url"];
    source = getUrlVars()["source"];

    var formates = ['SaveBox', 'Telly', 'WatchApne', 'Video'];
    $.each(formates, function (index, elem) {

        if (index != 0) {
            var od = $('#div-0');
            var cd = $(od).clone();

            $(cd).attr('id', 'div-' + index);
            cd.html(cd.html().replace('a-0', 'a-' + index).replace('span-0', 'span-' + index).replace('ui-btn-icon-right', '').replace('ui-corner-top', ''));

            $(od).parent().append(cd);
        }

        $('#a-' + index).attr('href', 'url.html?source=' + source + '&s=' + escape(show) + '&d=' + date + '&url=' + url + '&f=' + elem);
        $('#span-' + index).html(elem);
    });
    HideProgress('content');
}

function getMovie() {

    setContentDiv();

    var script = "http://mytvweb.azurewebsites.net/GetMovies.aspx";
    $.getScript(script, function (data, textStatus, jqxhr) {
        $.each(myTVScript.movies, function (index, elem) {

            if (index != 0) {
                var od = $('#div-0');
                var cd = $(od).clone();

                $(cd).attr('id', 'div-' + index);
                cd.html(cd.html().replace('a-0', 'a-' + index).replace('span-0', 'span-' + index).replace('ui-btn-icon-right', '').replace('ui-corner-top', ''));

                $(od).parent().append(cd);
            }

            $('#a-' + index).attr('href', 'movieurl.html?m=' + escape(elem.MovieURL));
            $('#span-' + index).html(elem.MovieName);
        });
    });
}

function playMovie() {
    var m = getUrlVars()["m"];
    var url = "http://mytvweb.azurewebsites.net/GetMovies.aspx?m=" + m;
    $.getScript(url, function (data, textStatus, jqxhr) {

        if (myTVScript == null || myTVScript.movies == null || myTVScript.movies == '')
            alert('No video found');
        else {

            var h = screen.height;
            var w = screen.width;
            var cookies = myTVScript.cookieString.split('^');
            for (p in cookies)
            {
                document.cookie = p;
            }

            jwplayer("videoplayer").setup({
                file: myTVScript.movies,
                autostart: 'true',
                width: '100%',
                aspectratio: '16:9',
                primary:'html5'
            });
        }
    });
}

function getRadio() {

    var formates = ['http://tunein.com/embed/player/s129208/', 'http://tunein.com/embed/player/s68483/'];
    $.each(formates, function (index, elem) {

        if (index != 0) {
            var od = $('#div-0');
            var cd = $(od).clone();

            $(cd).attr('id', 'div-' + index);
            cd.html(cd.html().replace('a-0', 'a-' + index).replace('span-0', 'span-' + index).replace('ui-btn-icon-right', '').replace('ui-corner-top', ''));

            $(od).parent().append(cd);
        }

        $('#a-' + index).attr('href', '#');
        $('#span-' + index).html('<iframe src="' + elem + '" />');
    });

}

function setContentDiv() {
    var h = screen.height;
    var w = screen.width;
}

function getUrlVars() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0]] = hash[1];
    }
    return vars;
}

function move(go) {
    if (go == 'd')
        $('#lstShow').scrollTop($('#lstShow').scrollTop() + 20);
    else
        $('#lstShow').scrollTop($('#lstShow').scrollTop() - 20);

}

function ShowProgress(elementId) {
    $("#" + elementId).css("position", "relative");

    if (!($("#" + elementId).find("div.progress-bar")[0])) {
        $("#" + elementId).prepend("<div class='progress-bar'><div class='progress-bar-img'>&nbsp;</div></div>");
    }
}

function HideProgress(elementId) {
    $("#" + elementId).find(".progress-bar").remove();
}
