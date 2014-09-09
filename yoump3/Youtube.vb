Imports Google.GData.Client
Imports Google.GData.Extensions
Imports Google.GData.Extensions.MediaRss
Imports Google.YouTube
Imports Google.GData.YouTube
Imports yoump3.Settings
Imports Google.GData.Extensions.Location
Imports System.Drawing


Public Class Youtube

    Public Class Uploader
        Public Req As Youtube
        Public File As String
        Public Title As String
        Public Description As String
        Public Keywords As String
        Public UpVid As Video
        Public Category As String
        Public IsPrivate As Boolean

        Public Sub New(ByRef yt As Youtube, ByVal file As String, ByVal title As String, ByVal desc As String, _
                       ByVal keywords As String, ByVal Category As String, _
                       Optional ByVal [private] As Boolean = False)
            Me.Category = Category
            Me.Req = yt
            Me.File = file
            Me.Title = title
            Me.Description = desc
            Me.Keywords = keywords
            Me.IsPrivate = [private]
        End Sub


        Public Sub Construct()
            If Not IO.File.Exists(Me.File) Then Return
            Me.UpVid = New Video
            UpVid.Title = Me.Title
            UpVid.Tags.Add(New MediaCategory(Me.Category, YouTubeNameTable.CategorySchema))
            UpVid.Keywords = Me.Keywords
            UpVid.Description = Me.Description
            UpVid.YouTubeEntry.Private = Me.IsPrivate
            UpVid.YouTubeEntry.Location = New GeoRssWhere(37, -122)
            ' alternatively, you could just specify a descriptive string
            ' newVideo.YouTubeEntry.setYouTubeExtension("location", "Mountain View, CA");
            UpVid.YouTubeEntry.MediaSource = New MediaFileSource(Me.File, "video")
        End Sub
        Public Function Upload() As Video
            Return Me.Req.Req.Upload(Me.UpVid)
        End Function

    End Class



    Public Enum VidStatus
        Uploaded
        Processing
        Failed
        Rejected
    End Enum

    Public Function GetVideoStatus(ByVal vid As Video) As VidStatus
        If (vid.IsDraft) Then
            If vid.Status.Name = "processing" Then : Return VidStatus.Processing
            ElseIf vid.Status.Name = "rejected" Then : Return VidStatus.Rejected
            ElseIf vid.Status.Name = "failed" Then : Return VidStatus.Failed
            Else : Return VidStatus.Uploaded
            End If
        End If
    End Function

    Public Sub SetVideoRating(ByVal vid As Video, Optional ByVal rating As Int16 = 1)
        vid.Rating = rating
        Req.Insert(vid.RatingsUri, vid)
    End Sub


    Public Function GetVideoComments(ByVal vid As Video) As List(Of Comment)
        Return New List(Of Comment)(Req.GetComments(vid))
    End Function


    Public Sub CommentVideo(ByVal vid As Video, ByVal comment As String, Optional ByVal isReply As Boolean = False, Optional ByVal replyTo As Comment = Nothing)
        Dim com As New Comment
        com.Content = comment
        If Not GetVideoComments(vid).Contains(replyTo) Then GoTo invalidReplyTo
        If isReply And replyTo IsNot Nothing Then com.ReplyTo(replyTo)
invalidReplyTo:
        Req.AddComment(vid, com)
    End Sub




    Public Settings As YouTubeRequestSettings
    Public Req As YouTubeRequest
    Public Sub New(ByVal user As String, ByVal pass As String)
        Settings = New YouTubeRequestSettings("Outshared", GDataKey, user, pass)
        Me.Req = New YouTubeRequest(Settings)
        [Global].Add("YOUTUBE_USER", user)
        [Global].Add("YOUTUBE_PASS", pass)
    End Sub
    Public Enum OrderBy As Int16
        relevance
        viewCount
        published
        rating
    End Enum


    'The OrderBy method sets the order in which to list entries, such as by relevance, viewCount, published, or rating.
    Public Function SearchVideo(ByVal videoQuery As String) As List(Of Video)
        If Req Is Nothing Then Return Nothing
        Dim qr As New YouTubeQuery(YouTubeQuery.DefaultVideoUri)

        qr.OrderBy = OrderBy.relevance.ToString
        qr.Query = videoQuery
        qr.SafeSearch = YouTubeQuery.SafeSearchValues.None
        Dim ret As Feed(Of Video) = Req.Get(Of Video)(qr)
        Return New List(Of Video)(ret.Entries)
    End Function

    Public Function GetMyVideos() As List(Of Video)
        If Req Is Nothing Then Return Nothing
        Return GetUserVideos()
    End Function

    Public Function GetUserVideos(Optional ByVal usrName As String = "default") As List(Of Video)
        If Req Is Nothing Then Return Nothing
        Return New List(Of Video)(Req.GetVideoFeed(usrName))
    End Function


    Public Function GetProfile(Optional ByVal name As String = "default") As ProfileEntry
        Dim uri As Uri = New Uri(UserUri.ToString & name)
        Dim prof As ProfileEntry = Req.Service.Get(uri.ToString)
        Return prof
    End Function

    Public Function GetVideoThumb(ByVal vid As Video) As String
        Return "http://img.youtube.com/vi/" & vid.VideoId & "/hqdefault.jpg"
    End Function

    Public Function GetVideoThumbImg(ByVal link As String) As Bitmap
        Dim wc As Bitmap = New System.Drawing.Bitmap(New IO.MemoryStream((New Net.WebClient).DownloadData(link)))
        Return wc
    End Function

    Public Function AddFriend(ByVal user As String)
        Dim feedUrl As String = "http://gdata.youtube.com/feeds/api/users/default/contacts"
        Dim newFriend As FriendsEntry = New FriendsEntry()
        newFriend.UserName = user
        newFriend.Categories.Add(New AtomCategory("Outshared friends", YouTubeNameTable.FriendsCategorySchema))
        Return Req.Service.Insert(New Uri(feedUrl), newFriend)
    End Function
    Public Function GetContacts(Optional ByVal user As String = "default") As FriendsFeed
        Dim uri As String = "http://gdata.youtube.com/feeds/api/users/" & user & "/contacts"
        Return Req.Service.GetFriends(New YouTubeQuery(uri))
    End Function
    Public Function GetMyContacts() As FriendsFeed
        Return GetContacts()
    End Function



    Public Function GetMyMessages() As MessageFeed
        If Req Is Nothing Then Return Nothing
        Dim url As String = "http://gdata.youtube.com/feeds/api/users/default/inbox"
        Return Req.Service.GetMessages(New YouTubeQuery(url))
    End Function
    Public Function SendMessage(ByVal [to] As String, ByVal txt As String, ByVal subject As String, ByVal video As Video)
        Dim toURI As String = "http://gdata.youtube.com/feeds/api/users/" & [to] & "/inbox"
        Dim videoEntry As YouTubeEntry = Nothing
        videoEntry = video.YouTubeEntry
        Dim msg As New MessageEntry
        msg.Title.Text = subject
        msg.Summary.Text = txt
        msg.Id = videoEntry.Id
        Return Req.Service.Insert(New Uri(toURI), msg)
    End Function


    Private STD_Feed As New Uri("http://gdata.youtube.com/feeds/api/standardfeeds/FEED_IDENTIFIER")
    Private MyFavs As New Uri("http://gdata.youtube.com/feeds/api/users/default/favorites")
    Private MyPlaylists As New Uri("http://gdata.youtube.com/feeds/api/users/default/playlists")
    Private MySubscriptions As New Uri("http://gdata.youtube.com/feeds/api/users/default/subscriptions")
    Private MyVideos As New Uri("http://gdata.youtube.com/feeds/api/users/default/videos")
    Private UserUri As New Uri("http://gdata.youtube.com/feeds/api/users/")
    Private MyNewSubscribedVideos As New Uri("https://gdata.youtube.com/feeds/api/users/default/newsubscriptionvideos")
    Private MyRecomendations As New Uri("https://gdata.youtube.com/feeds/api/users/default/recommendations")



    Public Function GetNewSubscribedVideos(Optional ByVal user As String = "default") As List(Of Video)
        Dim YTQ As New YouTubeQuery(MyNewSubscribedVideos.ToString.Replace("/default/", "/" & user & "/"))
        YTQ.OrderBy = OrderBy.published.ToString
        Dim vidFeed As Feed(Of Video) = Req.Get(Of Video)(YTQ)
        Return New List(Of Video)(vidFeed.Entries)
    End Function
    Public Function GetMyNewSubscribedVideos() As List(Of Video)
        Return GetNewSubscribedVideos()
    End Function
    Public Function GetRecommendations(Optional ByVal user As String = "default") As List(Of Video)
        Dim YTQ As New YouTubeQuery(MyRecomendations.ToString.Replace("/default/", "/" & user & "/"))
        YTQ.OrderBy = OrderBy.published.ToString
        Dim vidFeed As Feed(Of Video) = Req.Get(Of Video)(YTQ)
        Return New List(Of Video)(vidFeed.Entries)
    End Function
    Public Function GetMyRecommendations() As List(Of Video)
        Return GetRecommendations()
    End Function


    Public Function GetSubscriptions(Optional ByVal usrName As String = "default") As Feed(Of Subscription)
        Return Req.GetSubscriptionsFeed(usrName)
    End Function
    Public Function GetSubscriptionsL(Optional ByVal usrName As String = "default") As List(Of SubscriptionEntry)
        Return New List(Of SubscriptionEntry)(Req.GetSubscriptionsFeed(usrName))
    End Function
    Public Function GetMySubscriptions() As List(Of SubscriptionEntry)
        Return GetSubscriptionsL()
    End Function


    Public Function AddChannelSubscription(ByVal toChan As String) As Subscription
        Dim subscrb As New Subscription With {.UserName = toChan, .Type = SubscriptionEntry.SubscriptionType.channel}
        Return Req.Insert(Of Subscription)(MySubscriptions, subscrb)
    End Function

    Public Sub AddVideoToFavs(ByVal vid As Video)
        Req.Insert(Of Video)(MyFavs, vid)
    End Sub

    Public Enum STDFeedTypes As Int16
        most_viewed
        top_rated
        recently_featured
        watch_on_mobile
        most_discussed
        top_favourites
        top_responded
        most_recent
    End Enum
    Public Sub CreatePlaylist(ByVal name As String, ByVal description As String)
        Dim pl As New Playlist With {.Title = name, .Summary = description}
        Req.Insert(MyPlaylists, pl)
    End Sub
    Public Sub AddVideoToPlaylist(ByVal vid As Video, ByVal playlist As Playlist)
        Dim plVid As New PlayListMember With {.VideoId = vid.VideoId}
        Req.AddToPlaylist(playlist, plVid)
    End Sub


    Public Function GenSTDFeed(ByVal type As STDFeedTypes) As Uri
        Return New Uri(STD_Feed.ToString.Replace("FEED_IDENTIFIER", type.ToString))
    End Function
    Public Function GetStandart(ByVal type As STDFeedTypes) As Feed(Of Video)
        Dim uri As Uri = GenSTDFeed(type)
        Dim yqr As New YouTubeQuery(uri.ToString)
        yqr.OrderBy = "relevance"
        Dim ret As Feed(Of Video) = Req.Get(Of Video)(uri)
        Return ret
    End Function
    Public Function GetUsrFavs(ByVal usr As String) As Feed(Of Video)
        Return Req.GetFavoriteFeed(usr)
    End Function
    Public Sub RemoveVideo(ByVal video As Video)
        Req.Delete(video)
    End Sub

#Region "Playlists"
    Public Function GetUserPlaylists(Optional ByVal username As String = "default") As Feed(Of Playlist)
        Return Req.GetPlaylistsFeed(username)
    End Function
    Public Function GetMyPlaylists() As Feed(Of Playlist)
        Return GetUserPlaylists()
    End Function
    Public Function GetPlaylistItems(ByVal pl As Playlist) As List(Of PlayListMember)
        Return New List(Of PlayListMember)(Req.GetPlaylist(pl))
    End Function
#End Region


    Public Sub Flag(ByVal vid As Video, Optional ByVal flagType As ComplaintEntry.ComplaintType = ComplaintEntry.ComplaintType.PORN)
        Dim c As Complaint = New Complaint()
        c.Type = flagType
        c.Content = "This video offends my better sensibilities."
        Req.Insert(vid.ComplaintUri, c)
    End Sub




End Class
