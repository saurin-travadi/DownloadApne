
var source = "GetBollyStop";
//var root = "http://localhost:33619/";
var root = "http://mytvweb.azurewebsites.net/";

function getChannel() {

    setContentDiv();

    if (getUrlVars()["source"] != null)
        source = unescape(getUrlVars()["source"]);

    var script = root + "GetShow.aspx?source=" + source;
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

    var script = root + "GetShow.aspx?source=" + source;
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

    var script = root + "GetShow.aspx?source=" + source;
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

    var script = root + "GetDate.aspx?s=" + show + "&url=" + url + "&source=" + source;
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

function getVideo() {

    ShowProgress('showvideoplayer');

    var show = unescape(getUrlVars()["s"]);
    var date = unescape(getUrlVars()["d"]);
    var refurl = getUrlVars()["url"];
    var source = getUrlVars()["source"];
    var format = unescape(getUrlVars()["f"]);

    var url = root + "GetURL.aspx?s=" + show + "&d=" + date + "&source=" + source + "&url=" + refurl + "&f=" + format;

    $.getScript(url, function (data, textStatus, jqxhr) {

        HideProgress('showvideoplayer');

        if (myTVScript == null || myTVScript.url == null || myTVScript.url == '')
            alert('No video found');
        else {

            var h = screen.height - 200;
            var w = screen.width - 100;

            if (myTVScript.isDM == "true") {
                if (myTVScript.url.indexOf('html') >= 0 || myTVScript.url.indexOf('openload') >= 0) {
                    $('#a_watchhtml').attr('href', myTVScript.url);
                    $('#a_watchhtml').show();
                }
                else {
                    jwplayer("showvideoplayer").setup({
                        flashplayer: "http://assets-jpcust.jwpsrv.com/player/6/6124956/jwplayer.flash.swf",
                        html5player: "http://assets-jpcust.jwpsrv.com/player/6/6124956/jwplayer.html5.js",
                        file: myTVScript.url,
                        autoStart: false,
                        image: 'play.gif',
                        width: w - 20,
                        height: h - 20,
                        title: "Click Play",
                        primary: "html5"
                    });
                }
            }
            else {

                if (myTVScript.url1 == '') {

                    setupPlayer(myTVScript.url);
                }
                else {

                    $('#lstLink').show();
                    $('#lnk1').attr('src', myTVScript.url);
                    $('#lnk1').attr('src', myTVScript.url1);

                }
            }
        }
    });
}

function show(p) {
    setupPlayer(p == 1 ? myTVScript.url : myTVScript.url1);
}

var isVideoLoaded = false;
function setupPlayer(url) {

    var h = screen.height - 200;
    var w = screen.width - 100;

    if (isVideoLoaded) {

        var videocontainer = document.getElementById("video1");
        videocontainer.pause();
        videosource.setAttribute('src', url);
        videocontainer.load();
        videocontainer.play();

    }
    else {

        $('#video-container').show();

        $('#video1').append('<source id="videosource" type="video/mp4" src="' + url + '"></source>');
        $('#video1').css('height', h);
        $('#video1').css('width', w);
        $('#video_controls_bar').css('left', (w - 1400) / 2);

        init();
        isVideoLoaded = true;
    }
}

function getMovie() {

    setContentDiv();
    var script = root + "GetMovies.aspx";

    $.getScript(script, function (data, textStatus, jqxhr) {
        $.each(myTVScript.movies, function (index, elem) {

            if (index != 0) {
                var od = $('#div-0');
                var cd = $(od).clone();

                $(cd).attr('id', 'div-' + index);
                cd.html(cd.html().replace('a-0', 'a-' + index).replace('span-0', 'span-' + index).replace('ui-btn-icon-right', '').replace('ui-corner-top', ''));

                $(od).parent().append(cd);
            }
            $('#a-' + index).attr('href', 'url1.html?m=' + elem.MovieURL).attr("target", "_blank");
            $('#span-' + index).html(elem.MovieName);
        });

    });
}

function playMovie() {
    var m = getUrlVars()["m"];
    var url = root + "GetMovies.aspx?m=" + m;
    $.getScript(url, function (data, textStatus, jqxhr) {

        if (myTVScript == null || myTVScript.movies == null || myTVScript.movies == '')
            alert('No video found');
        else {

            var h = screen.height;
            var w = screen.width;
            var cookies = myTVScript.cookieString.split('^');
            for (p in cookies) {
                document.cookie = p;
            }

            jwplayer("videoplayer").setup({
                file: myTVScript.movies,
                autostart: 'true',
                width: '100%',
                aspectratio: '16:9',
                primary: 'html5'
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

function getSite() {
    var url = getUrlVars()["m"];

    $.getJSON('http://whateverorigin.org/get?url=' + encodeURIComponent(url) + '&callback=?', function (data) {
        data = data.contents;
        var reg = new RegExp("https://openload.co/embed/.+scrolling");
        var matches = data.match(reg);

        var str = matches[0];
        str = str.substring(0, str.length).replace('/"', '').replace('scrolling', '');
        window.location=str;
    });

    return false;
}