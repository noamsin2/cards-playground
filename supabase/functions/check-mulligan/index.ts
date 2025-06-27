// check-mulligan/index.ts
import { serve } from "https://deno.land/std@0.177.0/http/server.ts";
import { createClient } from "https://esm.sh/@supabase/supabase-js";
serve(async (req)=>{
  const { match_id, user_id } = await req.json();
  const supabase = createClient(Deno.env.get("SUPABASE_URL"), Deno.env.get("SUPABASE_SERVICE_ROLE_KEY"));
  // Update that this player has completed mulligan
  await supabase.from("match_players").update({
    has_mulliganed: true
  }).eq("match_id", match_id).eq("user_id", user_id);
  // Check if both players have mulliganed
  const { data: players, error } = await supabase.from("match_players").select("user_id, has_mulliganed").eq("match_id", match_id);
  if (error) {
    return new Response(JSON.stringify({
      error: error.message
    }), {
      status: 500
    });
  }
  if (players.length === 2 && players.every((p)=>p.has_mulliganed)) {
    // Randomly select a first player
    const firstPlayerID = players[Math.floor(Math.random() * 2)].user_id;
    // Update the match to start the game and set the first player
    await supabase.from("matches").update({
      game_state: "started",
      current_turn_index: firstPlayerID
    }).eq("match_id", match_id);
    return new Response(JSON.stringify({
      started: true,
      first_player_id: firstPlayerID
    }));
  }
  return new Response(JSON.stringify({
    started: false
  }));
});
