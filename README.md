# posts
posts micro-services repo

This micro service goal is to manage all the action that can be done on a post.

# Endpoints:

# /addPost:

body example:
{

    "token":"ewogICJhbGdvIjogIkhTMjU2IiwKICAidHlwZSI6ICJKV1QiCn0=.ewogICJpZCI6ICI2MjliN2FlMzAxNWFiYWQwOTMzZGQyZTgiLAogICJleHAiOiAiMCIKfQ==.Pz8/OCMQWAM/SUQ/eg4/Pwc/Py4/Pz8/Pz9IPz8qc1o=",
    
    "message":"Post de Jeff",
    
    "circles":[
        "id_circle_1",
        "id_circle_2"
        ],
        
    "media":"iVBORw0KGgoAAAANSUhEUgAABXgAAAOlCAYAAADXe4/8AAAgAElEQVR4nOzd34ttaX7f9/f3edbau6rOj+6Znh7PWLZ"
    
}

response example:

{

    "status": "success",
    
    "message": "post created successfully",
    
    "data": "629cb36ef44f9dac4f5bd649" //id of the post added
    
}

# /rmPost:

body example:

{

    "token":"ewogICJhbGdvIjogIkhTMjU2IiwKICAidHlwZSI6ICJKV1QiCn0=.ewogICJpZCI6ICI2MjliN2FlMzAxNWFiYWQwOTMzZGQyZTgiLAogICJleHAiOiAiMCIKfQ==.Pz8/OCMQWAM/SUQ/eg4/Pwc/Py4/Pz8/Pz9IPz8qc1o=",
    
    "id":"629cb048c21706951c4e85ea" //id of the post to remove
    
}

response example:

{

    "status": "success",
    
    "message": "post deleted successfully",
    
    "data": null
    
}

# /editPost

body example :

{

    "token":"ewogICJhbGdvIjogIkhTMjU2IiwKICAidHlwZSI6ICJKV1QiCn0=.ewogICJpZCI6ICI2MjliN2FlMzAxNWFiYWQwOTMzZGQyZTgiLAogICJleHAiOiAiMCIKfQ==.Pz8/OCMQWAM/SUQ/eg4/Pwc/Py4/Pz8/Pz9IPz8qc1o=",
    
    "id":"629cb36ef44f9dac4f5bd649",   //id of the post
    
    "circles":[
        "id_circle_1",   //new list of circles that will overwrite the old one
        "id_circle_3"
        ],
        
}

response example:

{

    "status": "success",
    
    "message": "post edited successfully",
    
    "data": null
    
}

# /vote

body example:

{

    "token":"ewogICJhbGdvIjogIkhTMjU2IiwKICAidHlwZSI6ICJKV1QiCn0=.ewogICJpZCI6ICI2MjliN2FlMzAxNWFiYWQwOTMzZGQyZTgiLAogICJleHAiOiAiMCIKfQ==.Pz8/OCMQWAM/SUQ/eg4/Pwc/Py4/Pz8/Pz9IPz8qc1o=",
    
    "id":"629cb36ef44f9dac4f5bd649",
    
    "direction":"up" // can be up or down
}

response example:

{

    "status": "success",
    
    "message": "post upvoted successfully",
    
    "data": null
}

# /getPost

body example:

{

    "token":"ewogICJhbGdvIjogIkhTMjU2IiwKICAidHlwZSI6ICJKV1QiCn0=.ewogICJpZCI6ICI2MjliN2FlMzAxNWFiYWQwOTMzZGQyZTgiLAogICJleHAiOiAiMCIKfQ==.Pz8/OCMQWAM/SUQ/eg4/Pwc/Py4/Pz8/Pz9IPz8qc1o=",
    
    "id":"629cb36ef44f9dac4f5bd649",
    
}

response example:

{

    "status": "success",
    
    "message": "success",
    
    "data": "{\"postid\":\"629cb36ef44f9dac4f5bd649\",\"userid\":\"629b7ae3015abad0933dd2e8\",\"message\":\"Post de Jeff\",\"date\":\"1654436718\",\"media\":\"UklGRtJhAABXRUJQVlA4IMZhAACwOAKdASp4BaUDPAIZaoMAAACWlu/BHj3qPRu0PbCP6Z+PnivyH4Z+g/4z9"
    
}
