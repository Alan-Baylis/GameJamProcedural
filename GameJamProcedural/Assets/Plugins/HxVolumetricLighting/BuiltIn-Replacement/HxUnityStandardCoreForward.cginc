#ifndef UNITY_STANDARD_CORE_FORWARD_INCLUDED
#define UNITY_STANDARD_CORE_FORWARD_INCLUDED

#if defined(UNITY_NO_FULL_STANDARD_SHADER)
#	define UNITY_STANDARD_SIMPLE 1
#endif

#include "UnityStandardConfig.cginc"
#include "HxVolumetricCore.cginc"



#if UNITY_STANDARD_SIMPLE

#include "HxUnityStandardCoreForwardSimple.cginc"

else
	VertexOutputBaseSimple vertBase (VertexInput v) { return vertForwardBaseSimple(v); }
	VertexOutputForwardAddSimple vertAdd (VertexInput v) { return vertForwardAddSimple(v); }
	half4 fragBase (VertexOutputBaseSimple i) : SV_Target { return fragForwardBaseSimpleInternal(i); }
	half4 fragAdd (VertexOutputForwardAddSimple i) : SV_Target { return fragForwardAddSimpleInternal(i); }
#else
#if UNITY_5_3_OR_NEWER
#include "HxUnityStandardCore.cginc"
VertexOutputForwardBase vertBase(VertexInput v) { return vertForwardBase(v); }
VertexOutputForwardAdd vertAdd(VertexInput v) { return vertForwardAdd(v); }
half4 fragBase(VertexOutputForwardBase i) : SV_Target{ return fragForwardBaseInternal(i); }
half4 fragAdd(VertexOutputForwardAdd i) : SV_Target{ return fragForwardAddInternal(i); }

#else
#include "HxUnityStandardCoreOld.cginc"
#endif
VertexOutputForwardBase vertBase(VertexInput v) { return vertForwardBase(v); }
VertexOutputForwardAdd vertAdd(VertexInput v) { return vertForwardAdd(v); }
half4 fragBase(VertexOutputForwardBase i) : SV_Target{ return fragForwardBase(i); }
half4 fragAdd(VertexOutputForwardAdd i) : SV_Target{ return fragForwardAdd(i); }
#endif

#endif // UNITY_STANDARD_CORE_FORWARD_INCLUDED
