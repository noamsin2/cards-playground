import { serve } from "https://deno.land/std/http/server.ts";
import { createClient } from "https://esm.sh/@supabase/supabase-js";
const SUPABASE_URL = Deno.env.get("SUPABASE_URL");
const SUPABASE_ANON_KEY = Deno.env.get("SUPABASE_ANON_KEY");
const SECRET_TOKEN = Deno.env.get("CRON_SECRET"); // your custom secret
serve(async (req)=>{
  // Check for your custom secret token in the header
  const token = req.headers.get("x-cron-secret");
  if (token !== SECRET_TOKEN) {
    return new Response(JSON.stringify({
      error: "Unauthorized"
    }), {
      status: 401,
      headers: {
        "Content-Type": "application/json"
      }
    });
  }
  const supabase = createClient(SUPABASE_URL, SUPABASE_ANON_KEY);
  const twoWeeksAgo = new Date(Date.now() - 14 * 24 * 60 * 60 * 1000).toISOString();
  const { data, error } = await supabase.from("CardGames").delete().lt("Deleted_At", twoWeeksAgo).eq("Is_Deleted", true);
  if (error) {
    return new Response(JSON.stringify({
      error
    }), {
      status: 500,
      headers: {
        "Content-Type": "application/json"
      }
    });
  }
  return new Response(JSON.stringify({
    deleted: data?.length || 0
  }), {
    status: 200,
    headers: {
      "Content-Type": "application/json"
    }
  });
});
