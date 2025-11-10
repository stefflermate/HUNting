import { useState } from "react";
import { Navigate } from "react-router-dom";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useAuth } from "@/context/AuthContext";
import { societiesApi } from "@/services/societiesApi";
import { Button } from "@/components/ui/button";
import { resolveAssetUrl } from "@/lib/apiClient";

export function SocietyManagementPage() {
  const { user, isAuthenticated } = useAuth();
  const queryClient = useQueryClient();
  const [title, setTitle] = useState("");
  const [content, setContent] = useState("");
  const [imageFile, setImageFile] = useState<File | null>(null);

  if (!isAuthenticated) {
    return <Navigate to="/auth/login" replace />;
  }

  if (user?.accountType !== "Society" || !user.ownedSociety) {
    return (
      <div className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 text-amber-100">
        Csak tarsasagi fiokkal, illetve a sajat tarsasag tulajdonosakent erheto el ez az oldal.
      </div>
    );
  }

  const societyId = user.ownedSociety.societyId;

  const membersQuery = useQuery({
    queryKey: ["society-members", societyId],
    queryFn: () => societiesApi.members(societyId),
  });

  const requestsQuery = useQuery({
    queryKey: ["society-requests", societyId],
    queryFn: () => societiesApi.requests(societyId),
  });

  const postsQuery = useQuery({
    queryKey: ["society-posts", societyId],
    queryFn: () => societiesApi.posts(societyId),
  });

  const approveMutation = useMutation({
    mutationFn: (candidateId: number) => societiesApi.approveRequest(societyId, candidateId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["society-requests", societyId] });
      queryClient.invalidateQueries({ queryKey: ["society-members", societyId] });
    },
  });

  const rejectMutation = useMutation({
    mutationFn: (candidateId: number) => societiesApi.rejectRequest(societyId, candidateId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["society-requests", societyId] });
    },
  });

  const postMutation = useMutation({
    mutationFn: () =>
      societiesApi.createPost(societyId, {
        title: title.trim() || undefined,
        content: content.trim() || undefined,
        image: imageFile ?? undefined,
      }),
    onSuccess: () => {
      setTitle("");
      setContent("");
      setImageFile(null);
      queryClient.invalidateQueries({ queryKey: ["society-posts", societyId] });
    },
  });

  const deletePostMutation = useMutation({
    mutationFn: (postId: number) => societiesApi.deletePost(societyId, postId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["society-posts", societyId] });
    },
  });

  return (
    <div className="space-y-8">
      <header className="rounded-3xl border border-amber-200/10 bg-[#121b12]/90 p-8 shadow-lg">
        <p className="text-sm uppercase tracking-[0.3em] text-amber-300/80">Tarsasag kezeles</p>
        <h1 className="font-display text-4xl text-amber-100">{user.ownedSociety.name}</h1>
        <p className="mt-3 text-sm text-amber-200/80">Tagok, jelentkezesek es belso posztok kezelese.</p>
      </header>

      <section className="grid gap-6 lg:grid-cols-2">
        <div className="rounded-3xl border border-amber-200/10 bg-[#121b12]/80 p-6">
          <h2 className="font-display text-2xl text-amber-100">Folyamatban levo kerelmek</h2>
          <div className="mt-4 space-y-3">
            {requestsQuery.data?.length === 0 && (
              <p className="text-sm text-amber-200/70">Nincs uj jelentkezes.</p>
            )}
            {requestsQuery.data?.map((request) => (
              <div
                key={request.userId}
                className="rounded-2xl border border-amber-400/10 bg-black/20 p-4 text-sm text-amber-100"
              >
                <p className="font-semibold">{request.user?.fullName ?? "Ismeretlen jelentkezo"}</p>
                <p className="text-xs uppercase tracking-[0.3em] text-amber-200/60">
                  {new Date(request.createdAt).toLocaleString("hu-HU")}
                </p>
                {request.message && (
                  <p className="mt-2 rounded-lg border border-amber-400/20 bg-black/20 p-2 text-amber-200/80">
                    {request.message}
                  </p>
                )}
                <div className="mt-3 flex gap-3">
                  <Button
                    size="sm"
                    className="bg-emerald-500 text-black hover:bg-emerald-400"
                    disabled={approveMutation.isPending}
                    onClick={() => approveMutation.mutate(request.userId)}
                  >
                    Elfogadas
                  </Button>
                  <Button
                    size="sm"
                    variant="outline"
                    className="border-red-400/60 text-red-200 hover:bg-red-400/10"
                    disabled={rejectMutation.isPending}
                    onClick={() => rejectMutation.mutate(request.userId)}
                  >
                    Elutasitas
                  </Button>
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="rounded-3xl border border-amber-200/10 bg-[#121b12]/80 p-6">
          <h2 className="font-display text-2xl text-amber-100">Tagok</h2>
          <div className="mt-4 space-y-3">
            {membersQuery.data?.map((member) => (
              <div
                key={member.userId}
                className="flex items-center justify-between rounded-2xl border border-amber-400/10 bg-black/20 px-4 py-3 text-sm text-amber-100"
              >
                <div>
                  <p className="font-semibold">{member.user?.fullName ?? "Ismeretlen tag"}</p>
                  <p className="text-xs uppercase tracking-[0.3em] text-amber-200/60">
                    {member.roleName ?? "Tag"}
                  </p>
                </div>
                <p className="text-xs text-amber-200/50">
                  {new Date(member.joinedAt).toLocaleDateString("hu-HU")}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="grid gap-6 lg:grid-cols-2">
        <form
          className="space-y-4 rounded-3xl border border-amber-200/10 bg-[#121b12]/80 p-6"
          onSubmit={(event) => {
            event.preventDefault();
            postMutation.mutate();
          }}
        >
          <h2 className="font-display text-2xl text-amber-100">Uj poszt</h2>
          <input
            placeholder="Poszt cime"
            value={title}
            onChange={(event) => setTitle(event.target.value)}
            className="w-full rounded-xl border border-amber-400/20 bg-black/20 px-4 py-3 text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
          />
          <textarea
            rows={5}
            placeholder="Ird le az uzenetet..."
            value={content}
            onChange={(event) => setContent(event.target.value)}
            className="w-full rounded-xl border border-amber-400/20 bg-black/20 px-4 py-3 text-sm text-amber-100 outline-none transition focus:border-amber-300 focus:ring-2 focus:ring-amber-300/40"
          />
          <input
            type="file"
            accept="image/*"
            onChange={(event) => setImageFile(event.target.files?.[0] ?? null)}
            className="text-sm text-amber-200/80 file:mr-3 file:rounded-lg file:border-0 file:bg-amber-400 file:px-4 file:py-2 file:text-sm file:font-semibold file:text-black"
          />
          <Button
            type="submit"
            className="w-full bg-amber-400 text-black hover:bg-amber-300"
            disabled={postMutation.isPending}
          >
            {postMutation.isPending ? "Poszt kuldese..." : "Poszt letrehozasa"}
          </Button>
        </form>

        <div className="space-y-4">
          <h2 className="font-display text-2xl text-amber-100">Legutobbi posztok</h2>
          {postsQuery.data?.map((post) => (
            <article
              key={post.societyPostId}
              className="space-y-2 rounded-3xl border border-amber-200/10 bg-[#121b12]/80 p-4"
            >
              <header className="flex items-center justify-between text-sm text-amber-200/70">
                <span>{new Date(post.createdAt).toLocaleString("hu-HU")}</span>
                <Button
                  size="sm"
                  variant="ghost"
                  className="text-red-300 hover:bg-red-400/10"
                  onClick={() => deletePostMutation.mutate(post.societyPostId)}
                >
                  Torles
                </Button>
              </header>
              {post.title && <h3 className="font-display text-xl text-amber-100">{post.title}</h3>}
              {post.content && <p className="text-sm text-amber-200/80">{post.content}</p>}
              {post.imageUrl && (
                <img
                  src={resolveAssetUrl(post.imageUrl)}
                  alt={post.title ?? "Poszt kep"}
                  className="max-h-64 w-full rounded-2xl object-cover"
                />
              )}
            </article>
          ))}
          {postsQuery.data?.length === 0 && (
            <p className="text-sm text-amber-200/70">Meg nincs poszt megosztva.</p>
          )}
        </div>
      </section>
    </div>
  );
}
